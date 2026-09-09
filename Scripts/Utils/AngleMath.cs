using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class AngleMath
    {
        /// <summary>
        /// Converts an angle to unsigned 0 to 360 range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToUnsigned360Range(float angle) => ((angle % 360) + 360) % 360;

        /// <summary>
        /// Converts an angle to signed -180 to 180 range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToSigned180Range(float angle)
        {
            float converted = ToUnsigned360Range(angle);
            return converted > 180f ? converted - 360 : converted;
        }
    }
}
