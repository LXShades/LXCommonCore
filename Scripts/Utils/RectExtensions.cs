using UnityEngine;

namespace LX.Common.Core
{
    public static class RectExtensions
    {
        /// <summary>
        /// Remaps a point from a specific position in the source rect to the equivalent position in the target rect. Pivot is normal between (0,0)-(1,1)
        /// </summary>
        public static Vector2 ToLocalRectPoint(this Vector2 point, in Rect targetRect, in Vector2 targetPivot)
        {
            return point - (targetRect.min + (targetRect.size * targetPivot));
        }
    }
}
