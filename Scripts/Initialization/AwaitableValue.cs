using System;
using System.Collections.Generic;
using UnityEngine;

namespace LX.Common.Core
{
    public struct StatefulAction
    {
        public struct ActionAwaiter
        {
            public UnityEngine.Object Obj;
            public Action Action;
        }

        /// <summary>
        /// If repeatable, the stateful action will fire whenever SetActive(true) is called, as well as if it is already active when someone subscribes
        /// </summary>
        /// <param name="inIsRepeatable"></param>
        public StatefulAction(bool inIsRepeatable)
        {
            isRepeatable = inIsRepeatable;
            IsActive = default;
            pendingActions = default;
        }

        public readonly bool isRepeatable;
        public bool IsActive { get; private set; }

        private List<ActionAwaiter> pendingActions;

        /// <summary>
        /// Calls Action as soon as this is Active, or immediately if this is already Active
        /// </summary>
        public void WhenTrue(UnityEngine.Object awaiter, Action action)
        {
            if (IsActive)
                action?.Invoke();

            if (!IsActive || isRepeatable)
                (pendingActions ??= new()).Add(new ActionAwaiter() { Obj = awaiter, Action = action });
        }

        /// <summary>
        /// Sets whether this StatefulAction is active. When active, all pending actions and future will run until deactivated.
        /// </summary>
        public void SetActive(bool inIsActive)
        {
            if (inIsActive && (!IsActive || isRepeatable))
            {
                if (pendingActions != null)
                {
                    foreach (ActionAwaiter actionAndAwaiter in pendingActions)
                    {
                        if (actionAndAwaiter.Obj)
                            actionAndAwaiter.Action?.Invoke();
                    }

                    if (!isRepeatable)
                    {
                        pendingActions = null;
                    }
                    else
                    {
                        // Keep the listener list tidy at least
                        pendingActions.RemoveAll(x => x.Obj == null);
                    }
                }
            }

            IsActive = inIsActive;
        }
    }

    /// <summary>
    /// Kinda another stateful action, but with a value that can be awaited. Sort of a V2 of the above, in hopes of being more intuitive.
    /// 
    /// Use extension functions WhenSet, WhenSetOrChanged, RemoveAwaiter and SetValue to control this.
    /// </summary>
    public struct AwaitableValue<TValue>
    {
        public struct Awaiter
        {
            public UnityEngine.Object Obj;
            public Action<TValue> ExecuteWhenSet;
            public bool IsWatchingAllFutureChanges;
        }

        public TValue Value;
        public bool IsSet { get; internal set; }

        internal const int kListSizeBeforeRegularTrimming = 10;

        internal List<Awaiter> awaiters;
    }

    public static class AwaitableValueExtensions
    {
        /// <summary>
        /// Calls Action as soon as this is Set, or immediately if this is already Set
        /// </summary>
        public static void WhenSet<TValue>(ref this AwaitableValue<TValue> awaitable, UnityEngine.Object awaiter, Action<TValue> executeWhenSet)
        {
            if (awaitable.IsSet)
                executeWhenSet?.Invoke(awaitable.Value);

            if (!awaitable.IsSet)
            {
                (awaitable.awaiters ??= new()).Add(new AwaitableValue<TValue>.Awaiter() { Obj = awaiter, ExecuteWhenSet = executeWhenSet });

                // Try and keep the list tidy regularly; don't want too much memory usage here
                if (awaitable.awaiters.Count > AwaitableValue<TValue>.kListSizeBeforeRegularTrimming)
                    awaitable.awaiters.RemoveAll(x => x.Obj == null);
            }
        }

        /// <summary>
        /// Calls Action as soon as this is Set, and whenever the value changes thereon to any other valid value.
        /// </summary>
        public static void WhenSetOrChanged<TValue>(ref this AwaitableValue<TValue> awaitable, UnityEngine.Object awaiter, Action<TValue> executeWhenSetOrChanged)
        {
            if (awaitable.IsSet)
                executeWhenSetOrChanged?.Invoke(awaitable.Value);

            (awaitable.awaiters ??= new()).Add(new AwaitableValue<TValue>.Awaiter() { Obj = awaiter, ExecuteWhenSet = executeWhenSetOrChanged, IsWatchingAllFutureChanges = true });

            // Try and keep the list tidy regularly; don't want too much memory usage here
            if (awaitable.awaiters.Count > AwaitableValue<TValue>.kListSizeBeforeRegularTrimming)
                awaitable.awaiters.RemoveAll(x => x.Obj == null);
        }

        /// <summary>
        /// Removes the action that was awaiting values via e.g. WhenSet or WhenSetOrChanged for the given object
        /// </summary>
        public static void RemoveAwaiter<TValue>(ref this AwaitableValue<TValue> awaitable, UnityEngine.Object awaiter)
        {
            if (awaitable.awaiters != null)
            {
                using var predicate = DisposablePredicate.Create((AwaitableValue<TValue>.Awaiter inList, UnityEngine.Object awaiterToRemove) => inList.Obj == awaiterToRemove, awaiter);
                awaitable.awaiters.RemoveAll(predicate.Call);
            }
        }

        /// <summary>
        /// Sets whether this StatefulAction is active. When set, all pending actions and future will run until deactivated.
        /// </summary>
        public static void SetValue<TValue>(ref this AwaitableValue<TValue> awaitable, bool isSet, in TValue value)
        {
            awaitable.Value = value;
            awaitable.IsSet = isSet;

            if (isSet)
            {
                if (awaitable.awaiters != null)
                {
                    foreach (AwaitableValue<TValue>.Awaiter actionAndAwaiter in awaitable.awaiters)
                    {
                        if (actionAndAwaiter.Obj)
                        {
                            try
                            {
                                actionAndAwaiter.ExecuteWhenSet?.Invoke(value);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }

                    // Remove all awaiters that are either not sticking around for future changes, or are dead
                    awaitable.awaiters.RemoveAll(x => !x.IsWatchingAllFutureChanges || x.Obj == null);
                }
            }
        }
    }
}
