using System;
using UnityEngine;

namespace LX.Common.Core
{
    /// <summary>
    /// Fires the delegate on the next frame, when requested
    /// </summary>
    public struct DeferrableAction
    {
        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.Object Owner;

        public bool HasOwner;

        /// <summary>
        /// The function to call after a frame. You can set this once, so that you're not instantiating it each time you fire it
        /// </summary>
        public Action FunctionToCall;

        private bool isPendingCall;

        /// <summary>
        /// Sets the function. Provide an owner so that the func can be suppressed if the object dies.
        /// </summary>
        public void SetFunc(UnityEngine.Object owner, Action func)
        {
            FunctionToCall = func;
            HasOwner = owner != null;
            Owner = owner;
        }

        /// <summary>
        /// Calls the action on the next frame
        /// </summary>
        public async void CallNextFrame()
        {
            if (!isPendingCall && LXLog.ValidOrError(FunctionToCall != null, "No FunctionToCall was set on this NextFrameAction"))
            {
                isPendingCall = true;
                await Awaitable.NextFrameAsync();
                if (!HasOwner || Owner != null)
                    FunctionToCall?.Invoke();
            }
        }
    }
}
