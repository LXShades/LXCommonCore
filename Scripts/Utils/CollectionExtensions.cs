using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class CollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this List<T> list, int index) => index >= 0 && index < list.Count;

        public static bool Contains<T>(this T[] theArray, T itemToFind) where T : class
        {
            foreach (T item in theArray)
            {
                if (item == itemToFind)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts an item at 0 in the array, and pushes the rest of the array up, discarding the final value
        /// </summary>
        public static void CircularInsertAtZero<T>(this T[] theArray, T itemToAppend)
        {
            for (int idx = theArray.Length - 1; idx >= 1; idx--)
                theArray[idx] = theArray[idx - 1];
            theArray[0] = itemToAppend;
        }
    }
}
