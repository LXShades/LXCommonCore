using UnityEngine;

namespace LX.Common.Core
{
    public static class KinematicMath
    {
        /// <summary>
        /// Converts a target height to a jump velocity required to reach that height, considering gravity. Gravity should be negative. Returns 0 if impossible to reach height
        /// </summary>
        public static float HeightToJumpVelocity(float targetHeight, float gravity) => -gravity * targetHeight > 0f ? Mathf.Sqrt(2f * -gravity * targetHeight) : 0f;
    }
}
