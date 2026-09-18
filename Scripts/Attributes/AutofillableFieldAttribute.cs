using System.Reflection;
using UnityEngine;

public class AutofillableFieldAttribute : PropertyAttribute
{
    public virtual bool canAutofill => false; // false on root class, can be true in e.g. AutofillableComponent

    // NOTE / TODO: autofillable was added after RequiredField to allow non-required, autofilled fields. However the archtecture is wonky. It would be nice if these were separated rather than a giant inheritance chain.
    public virtual bool isValueRequired => false;
    public bool isOnlyRequiredOnInstances { get; protected set; }

    public virtual bool TryAutofillField(object fieldTarget, FieldInfo field, out string error)
    {
        error = "(autofill feature not available)";
        return false;
    }
}
