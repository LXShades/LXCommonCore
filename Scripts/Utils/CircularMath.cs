using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class CircularMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CircularDifference(int from, int to, int circleSize) => to >= from ? to - from : circleSize - from + to;
    }
}
