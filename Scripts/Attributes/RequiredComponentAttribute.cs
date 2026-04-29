using System;
using System.Reflection;
using UnityEngine;

public class RequiredComponentAttribute : RequiredFieldAttribute
{
    public bool isAutoAssignable { get; private set; }
    public bool canAutoAssignFirstChild { get; private set; }

    public override bool canAutofill => isAutoAssignable;

    /// <summary>
    /// Attribute that ensures a component is readily available on an object. Depending on your parameters, the reference can be automatically assigned by the editor.
    /// </summary>
    /// <param name="isAutoAssignable">If true, the editor can auto-assign components</param>
    /// <param name="canAutoAssignFirstChild">If true, the editor can auto-assign the first component it finds in the object's children, if isAutoAssignable is also enabled</param>
    public RequiredComponentAttribute(bool isAutoAssignable = true, bool canAutoAssignFirstChild = false)
    {
        this.isAutoAssignable = isAutoAssignable;
        this.canAutoAssignFirstChild = canAutoAssignFirstChild;
    }

    public override bool TryAutofillField(object fieldTarget, FieldInfo field, out string error)
    {
        Component targetComponent = fieldTarget as Component;

        if (targetComponent)
        {
            Component foundMissingComponent = targetComponent.GetComponent(field.FieldType);

            if (foundMissingComponent == null && canAutoAssignFirstChild)
                foundMissingComponent = targetComponent.GetComponentInChildren(field.FieldType);

            if (foundMissingComponent)
            {
                error = null;
                field.SetValue(field, foundMissingComponent);
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