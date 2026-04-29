using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class LXLog
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidOrError(bool condition, string errorMessage)
        {
            if (!condition)
                Debug.LogError(errorMessage);
            return condition;
        }
    }
}
