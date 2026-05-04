using System.Runtime.CompilerServices;
using UnityEngine;
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
    }
}
