using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class CollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this List<T> list, int index) => index >= 0 && index < list.Count;
    }
}
