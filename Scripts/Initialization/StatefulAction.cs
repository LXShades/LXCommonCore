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
        /// Calls Action as soon as this is True, or immediately if this is already Active
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
}
