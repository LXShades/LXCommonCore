using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LX.Common.Core
{
    public static class LXLog
    {
        private static float spamResistantErrorCooldownSeconds = 5f;

        private static Dictionary<string, double> cooldownEndTimeByError = new();

#if UNITY_EDITOR
        private static bool isIgnoringAlerts = false;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode)
                isIgnoringAlerts = false;
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
        /// Programmer error that should not happen, but unlikely to break the application; application is probably continuable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrError<T1>(bool condition, string errorMessage, in T1 value1)
        {
            if (!condition)
                Debug.LogError($"{errorMessage} --- ({value1?.ToString()})");
            return condition;
        }

        /// <summary>
        /// Programmer error that should not happen, but unlikely to break the application; application is probably continuable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrError<T1, T2>(bool condition, string errorMessage, in T1 value1, in T2 value2)
        {
            if (!condition)
                Debug.LogError($"{errorMessage} --- ({value1?.ToString()}; {value2?.ToString()})");
            return condition;
        }

        /// <summary>
        /// Programmer error that should not happen, but unlikely to break the application; application is probably continuable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrError<T1, T2, T3>(bool condition, string errorMessage, in T1 value1, in T2 value2, in T3 value3)
        {
            if (!condition)
                Debug.LogError($"{errorMessage} --- ({value1?.ToString()}; {value2?.ToString()}; {value3?.ToString()})");
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
                if (!isIgnoringAlerts)
                {
                    switch (EditorUtility.DisplayDialogComplex("Breaking Assert", errorMessage, "Continue Once", "Continue and Ignore Rest", "Stop Game"))
                    {
                        case 0:
                            break;
                        case 1:
                            isIgnoringAlerts = true;
                            break;
                        case 2:
                            isIgnoringAlerts = true;
                            EditorApplication.isPlaying = false;
                            break;
                    }
                }
#endif
            }
            return condition;
        }

        /// <summary>
        /// Error that, if hit, will happen at a disruptively high frequency.
        /// These errors are emitted instantly first time, but suppressed and emitted at a specific maximum rate during future calls.
        /// </summary>
        public static void ErrorSpamResistant(string errorMessage)
        {
            if (!cooldownEndTimeByError.TryGetValue(errorMessage, out double cooldownEndTime) || Time.realtimeSinceStartupAsDouble >= cooldownEndTime)
            {
                cooldownEndTimeByError[errorMessage] = Time.realtimeSinceStartupAsDouble + spamResistantErrorCooldownSeconds;
                Debug.LogError(errorMessage);
            }
        }

        /// <summary>
        /// Logs variables
        /// Usage: LXLog.Vars(() => new ValueTuple(varA, varB, varC...) );
        /// </summary>
        public static void Vars<T>(Expression<Func<T>> expression)
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
                        names[idx] = Regex.Replace(methodCall.Arguments[idx].ToString(), "value\\(|\\+<>c__DisplayClass[0-9]*_[0-9]*\\)", "");
                    }
                }
                VarsImpl(names, values);
            }
            else
                VarsImpl(new[] { expression.ToString() }, new[] { expression.Compile()?.Invoke().ToString() });
        }

        private static void VarsImpl(string[] expressions, string[] values)
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
