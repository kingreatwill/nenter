using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nenter.Core.Extensions
{
    // <see cref="https://github.com/aspnet/EntityFrameworkCore/blob/release/3.1/src/EFCore/Extensions/EntityFrameworkQueryableExtensions.cs" />
    public static class QueryableExtensions
    {
        public static Task<List<TSource>> ToListAsync<TSource>([NotNull]this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<List<TSource>>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                tcs.SetResult(source.ToList());
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }
        
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TKey, TSource>([NotNull]this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<Dictionary<TKey, TSource>>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                tcs.SetResult(source.ToDictionary(keySelector));
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }
        
        public static Task<TSource[]> ToArrayAsync<TSource>([NotNull]this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<TSource[]>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                tcs.SetResult(source.ToArray());
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }
        
        public static Task LoadAsync<TSource>([NotNull]this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                while (source.GetEnumerator().MoveNext())
                {
                }

                tcs.SetResult(true);
            }
            catch (Exception exc)
            {
                (source as IDisposable)?.Dispose();

                tcs.SetException(exc);
            }

            return tcs.Task;
        }
        
       
        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<int>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                tcs.SetResult(source.Count());
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }

    }
}