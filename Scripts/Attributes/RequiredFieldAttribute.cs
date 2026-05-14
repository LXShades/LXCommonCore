using System.Reflection;
using UnityEngine;

public class RequiredFieldAttribute : PropertyAttribute
{
    public virtual bool canAutofill => false;
    public bool isOnlyRequiredOnInstances { get; protected set; }

    public virtual bool TryAutofillField(object fieldTarget, FieldInfo field, out string error)
    {
        error = "(autofill feature not available)";
        return false;
    }
}