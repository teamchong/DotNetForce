using System.Collections.Generic;

namespace DotNetForce
{
    internal class EnumerableChunk<T>
    {
        internal EnumerableChunk(IEnumerable<T> source, int size)
        {
            Source = source;
            Size = size;
        }

        protected IEnumerable<T> Source { get; set; }
        protected int Size { get; set; }

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
    }
}
