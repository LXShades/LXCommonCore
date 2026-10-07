using System.Reflection;
using UnityEngine;

public class RequiredFieldAttribute : AutofillableFieldAttribute
{
    public override bool isValueRequired => true;

    public RequiredFieldAttribute(bool isOnlyRequiredOnInstances = false)
    {
        this.isOnlyRequiredOnInstances = isOnlyRequiredOnInstances;
    }
}