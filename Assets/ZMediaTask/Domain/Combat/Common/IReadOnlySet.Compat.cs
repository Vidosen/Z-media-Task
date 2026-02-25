using System.Collections;

namespace System.Collections.Generic
{
    // Unity target profile in this project does not expose IReadOnlySet<T>.
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        bool Contains(T item);
    }

    public sealed class ReadOnlySet<T> : IReadOnlySet<T>
    {
        private readonly HashSet<T> _set;

        public ReadOnlySet(IEnumerable<T> items)
        {
            _set = items == null ? new HashSet<T>() : new HashSet<T>(items);
        }

        public int Count => _set.Count;

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
