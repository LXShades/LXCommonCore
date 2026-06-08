using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LX.Common.Core
{
    public class DisposablePool<T> where T : class, IDisposable
    {
        public static List<T> instancesAvailableForRent = new();
        private static List<GCHandle> instancesPendingDispose = new();

        [ThreadStatic]
        private static ListClearer listClearer = new();

        public class ListClearer
        {
            public System.Predicate<GCHandle> predicate;

            public T instance;

            public ListClearer() { predicate = Call; }

            public bool Call(GCHandle handle)
            {
                return handle.Target == instance;
            }
        }

        public static T RentExistingInstance()
        {
            if (instancesAvailableForRent.Count > 0)
            {
                T output = instancesAvailableForRent[instancesAvailableForRent.Count - 1];
                instancesAvailableForRent.RemoveAt(instancesAvailableForRent.Count - 1);
                AddInstanceToPendingDisposal(output);
                return output;
            }

            return null;
        }

        public static T RentNewInstance(T newInstance)
        {
            AddInstanceToPendingDisposal(newInstance);
            return newInstance;
        }

        public static T RentNewOrExistingInstance<A>() where A : T, new()
        {
            var existing = RentExistingInstance();
            if (existing == null)
                existing = RentNewInstance(new A());
            return existing;
        }

        private static void AddInstanceToPendingDisposal(T instance)
        {
            instancesPendingDispose.Add(GCHandle.Alloc(instance, GCHandleType.Weak));
        }

        public static void NotifyInstanceDisposed(T instance)
        {
            if (instancesPendingDispose.RemoveAll(x => x.Target == null) != 0)
                Debug.LogError($"A GenericDisposable was improperly disposed - it did not exist in our list ({typeof(T).FullName})");

            listClearer.instance = instance;
            instancesPendingDispose.RemoveAll(listClearer.predicate);
            instancesAvailableForRent.Add(instance);
        }
    }
}
