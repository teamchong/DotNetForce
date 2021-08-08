using System.Collections;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    internal static class EnumerableChunk
    {
        internal static EnumerableChunk<T> Create<T>(IEnumerable<T> source, int size)
        {
            return new EnumerableChunk<T> { Source = source, Size = size };
        }
    }

    internal class EnumerableChunk<T> : IEnumerable<IList<T>>
    {
        internal IEnumerable<T> Source { get; set; } = Enumerable.Empty<T>();
        internal int Size { get; set; }

        public IEnumerable<IList<T>> GetEnumerable()
        {
            var bucket = new List<T>();
            //var count = 0;

            foreach (var item in Source)
            {
                bucket.Add(item);
                if (bucket.Count < Size)
                    continue;

                yield return bucket;

                bucket = new List<T>();
                //count = 0;
            }

            if (bucket.Count > 0) yield return bucket;
        }

        public IEnumerator<IList<T>> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
