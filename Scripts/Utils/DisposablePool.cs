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
            instancesAvailableForRent.Add(newInstance);
            AddInstanceToPendingDisposal(newInstance);
            return newInstance;
        }

        private static void AddInstanceToPendingDisposal(T instance)
        {
            instancesPendingDispose.Add(GCHandle.Alloc(instance, GCHandleType.Weak));
        }

        public static void NotifyInstanceDisposed(T instance)
        {
            LXLog.ValidOrError(instancesPendingDispose.RemoveAll(x => x.Target == null) == 0, "A GenericDisposable was improperly disposed", typeof(T).FullName);

            instancesPendingDispose.RemoveAll(x => x.Target == instance);
            instancesAvailableForRent.Add(instance);
        }
    }
}
