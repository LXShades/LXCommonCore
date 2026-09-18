using System;
using System.Reflection;
using UnityEngine;

public class RequiredComponentAttribute : RequiredFieldAttribute
{
    public bool isAutoAssignable { get; private set; }
    public bool canAutoAssignFirstChild { get; private set; }
    public bool canAutoAssignFirstParent { get; private set; }

    public override bool canAutofill => isAutoAssignable;

    /// <summary>
    /// Attribute that ensures a component is readily available on an object. Depending on your parameters, the reference can be automatically assigned by the editor.
    /// </summary>
    /// <param name="isAutoAssignable">If true, the editor can auto-assign components</param>
    /// <param name="canAutoAssignFirstChild">If true, the editor can auto-assign the first component it finds in the object's children, if isAutoAssignable is also enabled</param>
    /// <param name="isOnlyRequiredOnInstances">If true, this value can be null on prefabs and is only required on instances of the object</param>
    public RequiredComponentAttribute(bool isAutoAssignable = true, bool canAutoAssignFirstChild = false, bool canAutoAssignFirstParent = false, bool isOnlyRequiredOnInstances = false)
    {
        this.isAutoAssignable = isAutoAssignable;
        this.canAutoAssignFirstChild = canAutoAssignFirstChild;
        this.canAutoAssignFirstParent = canAutoAssignFirstParent;
        this.isOnlyRequiredOnInstances = isOnlyRequiredOnInstances;
    }

    public override bool TryAutofillField(object fieldTarget, FieldInfo field, out string error) => AutofillableComponentAttribute.TryAutofillComponent(fieldTarget, field, out error, canAutoAssignFirstChild, canAutoAssignFirstParent);
}