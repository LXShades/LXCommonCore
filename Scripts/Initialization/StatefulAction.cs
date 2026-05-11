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

        public bool IsActive { get; private set; }

        private List<ActionAwaiter> pendingActions;

        /// <summary>
        /// Calls Action as soon as this is True, or immediately if this is already Active
        /// </summary>
        public void WhenTrue(UnityEngine.Object awaiter, Action action)
        {
            if (IsActive)
                action?.Invoke();
            else
                (pendingActions ??= new()).Add(new ActionAwaiter() { Obj = awaiter, Action = action });
        }

        /// <summary>
        /// Sets whether this StatefulAction is active. When active, all pending actions and future will run until deactivated.
        /// </summary>
        public void SetActive(bool inIsActive)
        {
            if (inIsActive && !IsActive)
            {
                if (pendingActions != null)
                {
                    foreach (ActionAwaiter actionAndAwaiter in pendingActions)
                    {
                        if (actionAndAwaiter.Obj)
                            actionAndAwaiter.Action?.Invoke();
                    }
                    pendingActions = null;
                }
            }

            IsActive = inIsActive;
        }
    }
}
