using UnityEngine;

namespace LX.Common.Core
{
    public static class RectExtensions
    {
        /// <summary>
        /// Converts a point to a relative point in the given rect. Useful for sprite pixels.
        /// </summary>
        public static Vector2 ToLocalRectPoint(this Vector2 point, in Rect targetRect, in Vector2 targetPivot)
        {
            return point - (targetRect.min + (targetRect.size * targetPivot));
        }
    }
}
