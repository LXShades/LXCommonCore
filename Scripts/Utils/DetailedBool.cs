using UnityEngine;

namespace LX.Common.Core
{
    public struct DetailedBool
    {
        public bool Value;
        public string FailureReason;

        public static implicit operator DetailedBool(bool value) => new DetailedBool() { Value = value, FailureReason = null };
        public static implicit operator bool(DetailedBool value) => value.Value;
        public static implicit operator DetailedBool(string failureReason) => new DetailedBool() { Value = false, FailureReason = failureReason };

        public void PrintAnyFailures()
        {
            if (!Value)
                Debug.LogError(FailureReason);
        }
    }
}
