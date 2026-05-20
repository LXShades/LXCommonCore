using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace LX.Common.Core
{
    // todo in general
    public class GenericDisposable<T>
    {
        public static List<GenericDisposable<T>> pooledInstances = new();

    }

    public class DisposableArray
    {
    }

    public class DisposableArray<T> : DisposableArray, IDisposable
    {
        protected static Dictionary<int, List<DisposableArray<T>>> arraysByTypeAndSize = new();

        private static List<GCHandle> arraysPendingDispose = new();

        public readonly T[] value;

        private const int kDefaultPoolCapacity = 4;

        /// <summary>
        /// Retrieves an uninitialized array of the given size from the pool
        /// </summary>
        public static DisposableArray<T> Create(int size)
        {
            List<DisposableArray<T>> arrays;
            if (!arraysByTypeAndSize.TryGetValue(size, out arrays))
            {
                arrays = new List<DisposableArray<T>>();
                arrays.Capacity = kDefaultPoolCapacity;
                arraysByTypeAndSize.Add(size, arrays);
            }

            // Find an existing array first
            if (arrays.Count > 0)
            {
                DisposableArray<T> arrayToReturn = arrays[arrays.Count - 1];

                arrays.RemoveAt(arrays.Count - 1);

                GCHandle gcHandle = GCHandle.Alloc(arrayToReturn, GCHandleType.Weak);
                arraysPendingDispose.Add(gcHandle);
                return arrays[arrays.Count - 1];
            }

            // Create a new one if all pooled arrays are in use
            DisposableArray<T> newArray = new(size);
            WeakReference<DisposableArray<T>> newWeakRef = new(newArray);
            arrays.Add(newArray);
            return newArray;
        }

        internal DisposableArray(int size)
        {
            value = new T[size];
        }

        public void Dispose()
        {
            int undisposedInstances = arraysPendingDispose.FindIndex(x => x.IsAllocated == false);

            if (undisposedInstances >= 0)
            {
                Debug.LogError("A DisposableArray exists that was not properly disposed. You should use the 'using' statement when allocating a DisposableArray.");

                arraysPendingDispose.RemoveAll(x =>
                {
                    if (x.IsAllocated == false)
                    {
                        x.Free();
                        return true;
                    }
                    return false;
                });
            }

            arraysPendingDispose.RemoveAll(x => x.Target == this);
            arraysByTypeAndSize[value.Length].Add(this);
        }
    }
}
