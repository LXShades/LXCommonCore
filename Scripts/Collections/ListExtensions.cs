using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LX.Common.Core
{
    public class NonGCEnumerator<T>
    {
        public struct ReadOnlyListEnumerator : IEnumerator<T>
        {
            public T Current => list[index];
            object IEnumerator.Current => Current;

            private IReadOnlyList<T> list;

            private int index;

            public ReadOnlyListEnumerator(IReadOnlyList<T> list)
            {
                this.list = list;
                index = -1;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                index++;
                return index < list.Count;
            }

            public void Reset() => index = -1;
        }

        public struct ReadOnlyListEnumerable
        {
            public IReadOnlyList<T> List;

            public ReadOnlyListEnumerable(IReadOnlyList<T> list) => List = list;
            public ReadOnlyListEnumerator GetEnumerator() => new ReadOnlyListEnumerator(List);
        }
    }


    public static class ListExtensions
    {
        public static NonGCEnumerator<T>.ReadOnlyListEnumerable AsNonGCEnumerable<T>(this IReadOnlyList<T> list) => new NonGCEnumerator<T>.ReadOnlyListEnumerable(list);
    }
}
