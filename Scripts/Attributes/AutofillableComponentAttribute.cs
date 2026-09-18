using System.Reflection;
using UnityEngine;

public class AutofillableComponentAttribute : AutofillableFieldAttribute
{
    public override bool canAutofill => true;

    public override bool TryAutofillField(object fieldTarget, FieldInfo field, out string error) => TryAutofillComponent(fieldTarget, field, out error, false, false);

    public static bool TryAutofillComponent(object fieldTarget, FieldInfo field, out string error, bool canAutoAssignFirstChild, bool canAutoAssignFirstParent)
    {
        Component targetComponent = fieldTarget as Component;

        if (targetComponent)
        {
            if (!typeof(Component).IsAssignableFrom(field.FieldType))
            {
                error = $"Field '{field.Name}' in {targetComponent.GetType().Name} (on {targetComponent.gameObject.name}) has a RequiredComponent attribute, but '{field.FieldType.Name}' is not a component class.";
                return false;
            }
            if (typeof(Transform).IsAssignableFrom(field.FieldType))
            {
                error = $"Field '{field.Name}' in {targetComponent.GetType().Name} (on {targetComponent.gameObject.name}) needs filling. By default, Transforms are not auto-filled, as all objects have their own Transform which is unlikely to be the one you want.";
                return false;
            }

            Component foundMissingComponent = targetComponent.GetComponent(field.FieldType);

            if (foundMissingComponent && targetComponent.GetComponents(field.FieldType).Length > 1)
            {
                error = $"{targetComponent.gameObject.name}'s {targetComponent.GetType().Name} found multiple possible component candidates for field '{field.Name}', so it cannot be reliably auto-filled.";
                return false;
            }

            if (foundMissingComponent == null && canAutoAssignFirstChild)
                foundMissingComponent = targetComponent.GetComponentInChildren(field.FieldType);
            if (foundMissingComponent == null && canAutoAssignFirstParent)
                foundMissingComponent = targetComponent.GetComponentInParent(field.FieldType);

            if (foundMissingComponent)
            {
                error = null;
                field.SetValue(fieldTarget, foundMissingComponent);
                return true;
            }
            else
            {
                error = $"{targetComponent.gameObject.name}'s {targetComponent.GetType().Name} could not find required {field.FieldType.Name} component for field '{field.Name}'";
                return false;
            }
        }
        error = "Object isn't a component";
        return false;
    }
}