using DotNetForce;
using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using DotNetForce.Common.Models.Xml;
using DotNetForce.Force;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace DotNetForce
{
    internal class EnumerableChunk<T>
    {
        protected IEnumerable<T> Source { get; set; }
        protected int Size { get; set; }

        internal EnumerableChunk(IEnumerable<T> source, int size)
        {
            Source = source;
            Size = size;
        }

        public IEnumerable<List<T>> GetEnumerable()
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

            if (bucket.Count > 0)
            {
                yield return bucket;
            }
        }
    }
}
