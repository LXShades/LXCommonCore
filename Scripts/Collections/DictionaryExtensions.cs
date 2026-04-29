using System.Collections.Generic;
using UnityEngine;

namespace LX.Common.Core
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Removes null keys from a dictionary and returns the number removed
        /// </summary>
        public static int RemoveNullKeys<A, B>(this Dictionary<A, B> dict)
        {
            List<A> toRemove = new List<A>();

            foreach (var key in dict.Keys)
            {
                if (key == null)
                    (toRemove ??= new List<A>()).Add(key);
            }

            if (toRemove != null)
            {
                foreach (var key in toRemove)
                    dict.Remove(key);
                return toRemove.Count;
            }

            return 0;
        }
    }
}
