using LX.Common.Core.Editor;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AutofillableFieldValidator : AssetModificationProcessor
{
    private struct RequiredFieldInfo
    {
        public KeyValuePair<FieldInfo, AutofillableFieldAttribute>[] requiredFields;
    }

    // todo: make sure refreshed if another assembly reloads
    private static Dictionary<System.Type, RequiredFieldInfo> componentReferenceInfoByType = new();

    private static readonly List<KeyValuePair<FieldInfo, AutofillableFieldAttribute>> tempFieldList = new();
    private static readonly List<Component> tempComponents = new();

    private static StringBuilder tempUnavailableComponentWarnings = new();

    [InitializeOnLoadMethod]
    private static void OnEditorInit()
    {
        PrefabStage.prefabSaving += (GameObject obj) => 
        {
            // Scan prefab
            ValidateAndAutofillRequiredFields(obj, true, out string errors);
            EditorValidator.instance.SetHasValidationError(obj, errors != null && errors.Length > 0, errors);
        };

        EditorValidator.OnAvailable(() => EditorValidator.instance.onPrePlayValidation += () => {
            // Scan all assets in scene before play
            List<GameObject> objectsToScan = new List<GameObject>();
            List<GameObject> nextObjectsToScan = new List<GameObject>(EditorSceneManager.GetActiveScene().GetRootGameObjects());

            while (nextObjectsToScan.Count > 0)
            {
                int num = nextObjectsToScan.Count;
                for (int i = 0; i < num; i++)
                {
                    objectsToScan.Add(nextObjectsToScan[i]);
                    for (int c = 0; c < nextObjectsToScan[i].transform.childCount; c++)
                        nextObjectsToScan.Add(nextObjectsToScan[i].transform.GetChild(c).gameObject);
                }
                nextObjectsToScan.RemoveRange(0, num);
            }

            foreach (GameObject obj in objectsToScan)
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                if (obj != null)
                {
                    // First validate the instance of the object. If the instance is fine, then we don't need to throw an error
                    bool wasObjectModified = ValidateAndAutofillRequiredFields(obj, true, out string errors);
                    EditorValidator.instance.SetHasValidationError(obj, errors != null && errors.Length > 0, errors);

                    if (prefab)
                    {
                        // Let's try and correct the prefab as well.
                        ValidateAndAutofillRequiredFields(prefab, true, out string secondaryErrors);

                        // We don't actually care if the prefab is correct as this is a scene scan. We need the _instances_ to be correct
                        // We're just applying our best fix to the source prefab, if we can. (this might get removed if it causes issues).
                    }
                }
            }
        });
    }

    public static bool ValidateAndAutofillRequiredField(Component componentWithReference, KeyValuePair<FieldInfo, AutofillableFieldAttribute> field, bool setDirtyIfChanged, out string error)
    {
        if (field.Value.isValueRequired && field.Value.isOnlyRequiredOnInstances && (PrefabUtility.IsPartOfPrefabAsset(componentWithReference.gameObject) || PrefabStageUtility.GetPrefabStage(componentWithReference.gameObject) != null))
        {
            // This value is expected or allowed to be null on prefabs
            error = null;
            return false; // was not modified
        }

        // Use Unity Object == operator if available
        object fieldValue = field.Key.GetValue(componentWithReference);
        if ((fieldValue is UnityEngine.Object unityObject && unityObject == null) || fieldValue == null)
        {
            if (field.Value.canAutofill)
            {
                if (field.Value.TryAutofillField(componentWithReference, field.Key, out error))
                {
                    if (setDirtyIfChanged)
                    {
                        // I don't know which one lool
                        EditorUtility.SetDirty(componentWithReference);
                        EditorUtility.SetDirty(componentWithReference.gameObject);
                    }

                    return true;
                }
            }

            if (field.Value.isValueRequired)
            {
                error = $"{componentWithReference.gameObject.name}'s {componentWithReference.GetType().Name} needs a user-provided value in '{field.Key.Name}' as it is a required field that cannot be auto-assigned.";
                return false;
            }
        }

        error = null;
        return false;
    }

    public static bool ValidateAndAutofillRequiredFields(GameObject targetObject, bool setDirtyIfChanged, out string warnings)
    {
        bool objectIsDirty = false;
        targetObject.GetComponentsInChildren(tempComponents);
        tempUnavailableComponentWarnings.Clear();

        foreach (Component targetComponent in tempComponents)
        {
            RequiredFieldInfo requiredFieldInfo = GetOrMakeRequiredFieldInfo(targetComponent.GetType());

            if (requiredFieldInfo.requiredFields != null)
            {
                foreach (KeyValuePair<FieldInfo, AutofillableFieldAttribute> field in requiredFieldInfo.requiredFields)
                {
                    objectIsDirty |= ValidateAndAutofillRequiredField(targetComponent, field, setDirtyIfChanged, out string error);

                    if (error != null && error.Length > 0)
                        tempUnavailableComponentWarnings.AppendLine(error);
                }
            }
        }

        warnings = tempUnavailableComponentWarnings.Length > 0 ? tempUnavailableComponentWarnings.ToString() : string.Empty;
        return objectIsDirty;
    }

    private static RequiredFieldInfo GetOrMakeRequiredFieldInfo(System.Type behaviourType)
    {
        if (!componentReferenceInfoByType.TryGetValue(behaviourType, out RequiredFieldInfo output))
        {
            tempFieldList.Clear();

            foreach (FieldInfo field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                AutofillableFieldAttribute autofillableFieldAttribute = field.GetCustomAttribute<AutofillableFieldAttribute>();
                if (autofillableFieldAttribute != null)
                    tempFieldList.Add(new KeyValuePair<FieldInfo, AutofillableFieldAttribute>(field, autofillableFieldAttribute));
            }

            if (tempFieldList.Count > 0)
                output.requiredFields = tempFieldList.ToArray();
            componentReferenceInfoByType[behaviourType] = output;
        }

        return output;
    }

}