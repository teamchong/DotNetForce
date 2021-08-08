using System;
using DotNetForce.Common.Models.Json;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DotNetForce
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public static class IAsyncEnumerableExtension
    {
        public static async IAsyncEnumerable<T> Pull<T>(this IAsyncEnumerable<QueryResult<T>>? source)
        {
            if (source == null) yield break;
            await foreach (var result in source
                .ConfigureAwait(false))
                if (result?.Records != null)
                    foreach (var record in result.Records)
                        yield return record;
        }

        public static IObservable<T> Push<T>(this IAsyncEnumerable<QueryResult<T>>? source)
        {
            return Observable.Create<T>(async obs =>
            {
                try
                {
                    if (source != null)
                        await foreach (var record in source.Pull()
                            .ConfigureAwait(false))
                            obs.OnNext(record);
                    obs.OnCompleted();
                }
                catch (Exception exception)
                {
                    obs.OnError(exception);
                }
            });
        }
    }
}
