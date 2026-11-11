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

        /// <summary>
        /// Returns a rect with coordinates adjusted to fit the given aspect ratio in the centre
        /// </summary>
        public static Rect WithAspectFit(this Rect rect, float aspect)
        {
            Rect outRect = rect;
            if ((float)outRect.width / outRect.height > aspect)
            {
                float difference = outRect.height * aspect - outRect.width;
                outRect.xMin -= difference / 2;
                outRect.xMax += difference / 2;
            }
            else
            {
                float difference = outRect.width / aspect - outRect.height;
                outRect.yMin -= difference / 2;
                outRect.yMax += difference / 2;
            }

            return outRect;
        }
    }
}
