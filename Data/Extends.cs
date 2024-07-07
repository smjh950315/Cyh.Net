using Cyh.Net.Models;
using Cyh.Net.Native;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
namespace Cyh.Net.Data {
    public static partial class Extends {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CanRead<T, Dto>([NotNullWhen(true)] this IDtoHandler<T, Dto>? handler, [NotNullWhen(true)] IReadOnlyDataSource<T>? srcReadAccesser, [NotNullWhen(true)] out IQueryable<Dto>? dstReadAccesser) {
            dstReadAccesser = null;
            if (srcReadAccesser == null || handler == null) { return false; }
            if (srcReadAccesser.ReadOnlyAccesser == null) { return false; }
            dstReadAccesser = srcReadAccesser.ReadOnlyAccesser.Select(handler.ExprToDTO);
            return dstReadAccesser != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CanWrite<T, Dto>([NotNullWhen(true)] this IDtoHandler<T, Dto>? handler, [NotNullWhen(true)] IWritableDataSource<T>? srcWriteAccesser) {
            return handler != null && srcWriteAccesser != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void TrySetResCode(this IDataTransResult? transResult, ResultEnum resultEnum, string? message = null) {
            if (transResult == null) { return; }
            transResult.ResultCode = resultEnum;
            transResult.Executed = DateTime.Now;
            transResult.Message = message;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IQueryable<U>? TryUse<T, U>(this IQueryable<T>? readAccesser, IDtoHandler<T, U>? handler) {
            if (readAccesser == null) { return null; }
            if (handler == null) { return null; }
            if (handler.ExprToDTO == null) { return null; }
            return readAccesser.Select(handler.ExprToDTO);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IQueryable<T> TryUse<T>(this IQueryable<T> readAccesser, Expression<Func<T, bool>>? filter) {
            return filter == null ? readAccesser : readAccesser.Where(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<T> TryUse<T, TKey>(this IQueryable<T> readAccesser, Func<T, TKey>? order) {
            return order == null ? readAccesser : readAccesser.OrderBy(order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<T> TryUse<T>(this IEnumerable<T> readAccesser, DataRange? dataRange) {
            return dataRange == null ? readAccesser : readAccesser.Skip(dataRange.Begin).Take(dataRange.Count);
        }

        public static void text<T, Dto>(IDataSource<T> src, IDtoHandler<T, Dto> dtoHandler) where Dto : LifeTimeHandler {

            IDataRepository<Dto> dataRepository = DataRepository.GetDataRepository(src, dtoHandler);

            dataRepository.GetAll(d => d != null, (1, 2));

        }
    }
}
