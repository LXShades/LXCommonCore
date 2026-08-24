using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LX.Common.Core
{
    public static class AssignIfChanged
    {
        /// <summary>
        /// Sets a value to the new value, if it's different. Returns true if the value changes. This is useful for properties where you want to just set them every frame, but are not sure how performant it is to set it if the value hasn't actually changed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AssignAndChange<T>(this ref T value, in T newValue) where T : struct, IEquatable<T>
        {
            if (!value.Equals(newValue))
            {
                value = newValue;
                return true;
            }
            return false;
        }
    }
}
