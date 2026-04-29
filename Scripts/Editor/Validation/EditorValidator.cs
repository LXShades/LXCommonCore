using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LX.Common.Core.Editor
{
    /// <summary>
    /// Helper for editor validation stuff. Warns the user explicitly if there's a validation error of concern
    /// </summary>
    public class EditorValidator
    {
        public static EditorValidator instance { get; private set; }

        private int numValidationErrorsToShowWhenAlerting = 3;

        private Dictionary<GameObject, string> validationErrorsByObject = new();

        public System.Action onPrePlayValidation;

        private static List<Action> onAvailableActions = new();

        [UnityEditor.InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            instance = new EditorValidator();

            foreach (var action in onAvailableActions)
                action();
            onAvailableActions.Clear();
        }

        EditorValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnAvailable(Action action)
        {
            if (instance != null)
                action?.Invoke();
            else if (action != null)
                onAvailableActions.Add(action);
        }

        public void SetHasValidationError(GameObject impactedObject, bool hasError, string errorString)
        {
            if (hasError)
            {
                // We want errors to persist until the user starts the game
                validationErrorsByObject[impactedObject] = errorString;
            }
            else
            {
                validationErrorsByObject.Remove(impactedObject);
            }

            // Might need spam preotection later
            if (hasError)
                Debug.LogError(errorString, impactedObject);
        }

        private void CleanEmptyErrorEntries()
        {
            validationErrorsByObject.RemoveNullKeys();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.ExitingEditMode)
            {
                CleanEmptyErrorEntries();

                try
                {
                    onPrePlayValidation?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (validationErrorsByObject.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("There are validation errors. Are you sure you want to run?\n\n");
                    sb.Append("Number of objects with errors: ");
                    sb.Append(validationErrorsByObject.Count);
                    if (validationErrorsByObject.Count > numValidationErrorsToShowWhenAlerting)
                        sb.Append($"({numValidationErrorsToShowWhenAlerting} shown):");
                    sb.Append("\n");

                    Dictionary<GameObject, string>.Enumerator valueEnumerator = validationErrorsByObject.GetEnumerator();
                    for (int i = 0; i < Mathf.Min(numValidationErrorsToShowWhenAlerting, validationErrorsByObject.Count); i++)
                    {
                        valueEnumerator.MoveNext();
                        sb.AppendLine(valueEnumerator.Current.Value);
                    }

                    if (!EditorUtility.DisplayDialog("Validation Errors in Project", sb.ToString(), "Yes", "No"))
                        EditorApplication.isPlaying = false;
                }
                }
        }
    }
}
