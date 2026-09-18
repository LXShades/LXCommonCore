using System.Reflection;
using UnityEngine;

public class RequiredFieldAttribute : AutofillableFieldAttribute
{
    public override bool isValueRequired => true;
}