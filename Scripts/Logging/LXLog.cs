using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Text;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LX.Common.Core
{
    public static class LXLog
    {
#if UNITY_EDITOR
        private static bool isIgnoringAsserts = false;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode)
                isIgnoringAsserts = false;
        }
#endif

        /// <summary>
        /// Programmer error that should not happen, but unlikely to break the application; application is probably continuable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrError(bool condition, string errorMessage)
        {
            if (!condition)
                Debug.LogError(errorMessage);
            return condition;
        }

        /// <summary>
        /// Programmer error that should not happen and indicates an immediate issue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrAlert(bool condition, string errorMessage)
        {
            if (!condition)
            {
                Debug.LogError(errorMessage);

#if UNITY_EDITOR
                if (!isIgnoringAsserts)
                {
                    switch (EditorUtility.DisplayDialogComplex("Breaking Assert", errorMessage, "Continue Once", "Continue and Ignore Rest", "Stop Game"))
                    {
                        case 0:
                            break;
                        case 1:
                            isIgnoringAsserts = true;
                            break;
                        case 2:
                            isIgnoringAsserts = true;
                            EditorApplication.isPlaying = false;
                            break;
                    }
                }
#endif
            }
            return condition;
        }

        public static void LogVars<T>(Expression<Func<T>> expression)
        {
            var expressionResult = expression.Compile().Invoke();

            if (expression.Body is MethodCallExpression methodCall && expressionResult.GetType().Name.Contains("ValueTuple"))
            {
                var tupleType = expressionResult.GetType();
                string[] values = new string[methodCall.Arguments.Count];
                string[] names = new string[methodCall.Arguments.Count];

                for (int idx = 0; idx < methodCall.Arguments.Count; idx++)
                {
                    if (tupleType.GetField($"Item{idx + 1}") is FieldInfo field)
                    {
                        values[idx] = field.GetValue(expressionResult).ToString();
                        names[idx] = methodCall.Arguments[idx].ToString();
                    }
                }
                LogVarsImpl(names, values);
            }
            else
                LogVarsImpl(new[] { expression.ToString() }, new[] { expression.Compile()?.Invoke().ToString() });
        }

        private static void LogVarsImpl(string[] expressions, string[] values)
        {
            StringBuilder sb = new();
            for (int idx = 0; idx < expressions.Length; idx++)
            {
                if (expressions[idx] != null && values[idx] != null)
                {
                    int startChar = Mathf.Max(expressions[idx].IndexOf("=>"), expressions[idx].IndexOf("{"), 0);

                    sb.AppendLine($"{expressions[idx].Substring(startChar).Trim()}: {values[idx]}");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
