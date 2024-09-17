using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ComponentReferenceValidator : AssetModificationProcessor
{
    private struct ComponentReferenceInfo
    {
        public FieldInfo[] componentReferenceFields;
    }

    // todo: make sure refreshed if another assembly reloads
    private static Dictionary<System.Type, ComponentReferenceInfo> componentReferenceInfoByType = new Dictionary<System.Type, ComponentReferenceInfo>();

    private static readonly List<FieldInfo> tempFieldList = new List<FieldInfo>();
    private static readonly List<Component> tempComponents = new List<Component>();

    private static StringBuilder tempUnavailableComponentWarnings = new StringBuilder();

    [InitializeOnLoadMethod]
    private static void OnEditorInit()
    {
        PrefabStage.prefabSaving += (GameObject obj) => 
        {
            if (FindAndApplyComponentReferences(obj, out string errors))
                EditorUtility.SetDirty(obj);

            if (!string.IsNullOrEmpty(errors))
                Debug.LogError(errors);
        };
    }

    private static bool FindAndApplyComponentReferences(GameObject targetObject, out string warnings)
    {
        bool objectIsDirty = false;
        targetObject.GetComponentsInChildren(tempComponents);
        tempUnavailableComponentWarnings.Clear();

        foreach (Component component in tempComponents)
        {
            ComponentReferenceInfo componentRefs = GetOrMakeComponentReferenceInfo(component.GetType());

            if (componentRefs.componentReferenceFields != null)
            {
                foreach (FieldInfo field in componentRefs.componentReferenceFields)
                {
                    if ((UnityEngine.Object)field.GetValue(component) == null)
                    {
                        if (component.TryGetComponent(field.FieldType, out Component fieldComponent))
                        {
                            field.SetValue(component, fieldComponent);
                            objectIsDirty = true;
                        }
                        else
                        {
                            tempUnavailableComponentWarnings.AppendLine($"Object {targetObject.name}'s {component.GetType().Name} could not find required {field.FieldType.Name} component for field {field.Name}");
                        }
                    }
                }
            }
        }

        warnings = tempUnavailableComponentWarnings.Length > 0 ? tempUnavailableComponentWarnings.ToString() : string.Empty;
        return objectIsDirty;
    }

    private static ComponentReferenceInfo GetOrMakeComponentReferenceInfo(System.Type behaviourType)
    {
        if (!componentReferenceInfoByType.TryGetValue(behaviourType, out ComponentReferenceInfo output))
        {
            tempFieldList.Clear();

            foreach (FieldInfo field in behaviourType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                ComponentReferenceAttribute componentReferenceAttribute = field.GetCustomAttribute<ComponentReferenceAttribute>();
                if (componentReferenceAttribute != null)
                    tempFieldList.Add(field);
            }

            if (tempFieldList.Count > 0)
                output.componentReferenceFields = tempFieldList.ToArray();
            componentReferenceInfoByType[behaviourType] = output;
        }

        return output;
    }

}