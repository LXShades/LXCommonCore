using System;
using UnityEngine;

namespace LX.Common.Core
{
    public class DisposablePredicate<Arg1, Capture1> : IDisposable
    {
        System.Predicate<Arg1> caller;
        public Func<Arg1, Capture1, bool> func;
        Capture1 capture;

        public DisposablePredicate() => caller = Call;

        public bool Call(Arg1 arg) => func(arg, capture);

        public void Dispose() => DisposablePool<DisposablePredicate<Arg1, Capture1>>.NotifyInstanceDisposed(this);

        public static implicit operator System.Predicate<Arg1>(DisposablePredicate<Arg1, Capture1> container) => container.caller;
    }

    public class DisposablePredicate
    {
        public static DisposablePredicate<Arg1, Capture1> Create<Arg1, Capture1>(Func<Arg1, Capture1, bool> action, in Capture1 capture)
        {
            var predicate = DisposablePool<DisposablePredicate<Arg1, Capture1>>.RentNewOrExistingInstance<DisposablePredicate<Arg1, Capture1>>();
            predicate.func = action;
            return predicate;
        }
    }
}
