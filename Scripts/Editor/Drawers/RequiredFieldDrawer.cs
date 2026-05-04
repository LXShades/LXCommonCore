using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LX.Common.Core.Editor
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute), true)]
    public class RequiredFieldDrawer : PropertyDrawer
    {
        private int errorInfoBoxHeight = 16;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string errors = null;
            if (property.serializedObject.targetObject is MonoBehaviour targetBehaviour)
            {
                Type objectType = property.serializedObject.targetObject.GetType();
                FieldInfo field = objectType.GetField(property.name);

                if (field != null)
                    RequiredFieldValidator.ValidateAndAutofillRequiredField(targetBehaviour, new KeyValuePair<FieldInfo, RequiredFieldAttribute>(field, attribute as RequiredFieldAttribute), true, out errors);
            }

            var oldColor = GUI.color;
            bool hasError = errors != null && errors.Length > 0;
            if (hasError)
            {
                EditorGUI.DrawRect(position, new Color(0.2f, 0f, 0f));
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 16), "Field validation failed: This needs a valid value!", MessageType.Error);
                GUI.color = new Color(0.9f, 0.2f, 0.2f);
            }

            EditorGUI.PropertyField(new Rect(position.x, hasError ? position.y + errorInfoBoxHeight : position.y, position.width, hasError ? position.height - errorInfoBoxHeight : position.height), property, true);
            GUI.color = oldColor;
        }

        private bool ValidateAndAutofill(SerializedProperty property, out string errors)
        {
            if (property.serializedObject.targetObject is MonoBehaviour targetBehaviour)
            {
                Type objectType = property.serializedObject.targetObject.GetType();
                FieldInfo field = objectType.GetField(property.name);

                if (field != null)
                    return RequiredFieldValidator.ValidateAndAutofillRequiredField(targetBehaviour, new KeyValuePair<FieldInfo, RequiredFieldAttribute>(field, attribute as RequiredFieldAttribute), true, out errors);
            }
            errors = null;
            return false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ValidateAndAutofill(property, out string errors);

            if (errors != null && errors.Length > 0)
                return EditorGUI.GetPropertyHeight(property) + errorInfoBoxHeight;
            else
                return EditorGUI.GetPropertyHeight(property);
        }
    }
}
