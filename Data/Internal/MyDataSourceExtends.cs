using Cyh.Net.Data.Models;

namespace Cyh.Net.Data.Internal
{
    using Cyh.Net.Data.Logs;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal static class MyDataSourceExtends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T? First<T>(IQueryable<T> values, Expression<Func<T, bool>>? filter_expr) {
            if (filter_expr == null) {
                return values.FirstOrDefault();
            } else {
                return values.FirstOrDefault(filter_expr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IQueryable<T> All<T>(IQueryable<T> values, Expression<Func<T, bool>>? filter_expr) {
            if (filter_expr == null) {
                return values;
            } else {
                return values.Where(filter_expr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IQueryable<T> All<T>(IQueryable<T> values, DataRange? dataRange) {
            if (dataRange == null) {
                return values;
            } else {
                return values.Skip(dataRange.Begin).Take(dataRange.Count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IQueryable<T> All<T>(IQueryable<T> values, Expression<Func<T, bool>>? filter_expr, DataRange? dataRange) {
            return All(All(values, filter_expr), dataRange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IQueryable<T> All_Asc<T, TKey>(IQueryable<T> values, Expression<Func<T, bool>>? filter_expr, Expression<Func<T, TKey>>? order, DataRange? dataRange) {
            if (order == null) {
                return All(All(values, filter_expr), dataRange);
            } else {
                return All(All(values, filter_expr).OrderBy(order), dataRange);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IQueryable<T> All_Desc<T, TKey>(IQueryable<T> values, Expression<Func<T, bool>>? filter_expr, Expression<Func<T, TKey>>? order, DataRange? dataRange) {
            if (order == null) {
                return All(All(values, filter_expr), dataRange);
            } else {
                return All(All(values, filter_expr).OrderByDescending(order), dataRange);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasAny<T>(IQueryable<T> queryable, Expression<Func<T, bool>>? filter_expr) {
            if (filter_expr == null) {
                return queryable.Any();
            } else {
                return queryable.Any(filter_expr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount<T>(IQueryable<T> queryable, Expression<Func<T, bool>>? filter_expr) {
            if (filter_expr == null) {
                return queryable.Count();
            } else {
                return queryable.Count(filter_expr);
            }
        }
        internal static bool HasAny<T, V>(this IMyDataSource<T> dataSource, Expression<Func<T, V>> convert_expr, Expression<Func<V, bool>>? filter_expr) {
            if (dataSource.Queryable == null) { return false; }
            if (filter_expr == null) { return dataSource.Queryable.Any(); }
            return HasAny(dataSource.Queryable.Select(convert_expr), filter_expr);
        }
        internal static int GetCount<T, V>(this IMyDataSource<T> dataSource, Expression<Func<T, V>> convert_expr, Expression<Func<V, bool>>? filter_expr) {
            if (dataSource.Queryable == null) { return 0; }
            if (filter_expr == null) { return dataSource.Queryable.Count(); }
            return GetCount(dataSource.Queryable.Select(convert_expr), filter_expr);
        }
        internal static V? GetSingle<T, V>(
            this IMyDataSource<T> dataSource,
            Expression<Func<T, V>> convert_expr,
            Expression<Func<V, bool>>? filter_expr,
            DataTransResult? result) {
            if (dataSource.Queryable == null) { return default; }
            return First(dataSource.Queryable.Select(convert_expr), filter_expr);
        }
        internal static IEnumerable<V> GetMultiple<T, V>(
            this IMyDataSource<T> dataSource,
            Expression<Func<T, V>> convert_expr,
            Expression<Func<V, bool>>? filter_expr,
            DataRange? dataRange,
            DataTransResult? result) {
            if (dataSource.Queryable == null) { return []; }
            return All(dataSource.Queryable.Select(convert_expr), filter_expr, dataRange);
        }
        internal static IEnumerable<V> GetMultipleAsc<T, V, VKey>(
            this IMyDataSource<T> dataSource,
            Expression<Func<T, V>> convert_expr,
            Expression<Func<V, bool>>? filter_expr,
            Expression<Func<V, VKey>>? order_expr,
            DataRange? dataRange,
            DataTransResult? result) {
            if (dataSource.Queryable == null) { return []; }
            return All_Asc(dataSource.Queryable.Select(convert_expr), filter_expr, order_expr, dataRange);
        }
        internal static IEnumerable<V> GetMultipleDesc<T, V, VKey>(
            this IMyDataSource<T> dataSource,
            Expression<Func<T, V>> convert_expr,
            Expression<Func<V, bool>>? filter_expr,
            Expression<Func<V, VKey>>? order_expr,
            DataRange? dataRange,
            DataTransResult? result) {
            if (dataSource.Queryable == null) { return []; }
            return All_Desc(dataSource.Queryable.Select(convert_expr), filter_expr, order_expr, dataRange);
        }
        internal static bool AddSingle<T, V>(
            this IMyDataSource<T> dataSource,
            Func<V, T> converter,
            V? value,
            DataTransResult? result,
            bool exec_now) {
            if (value == null) {
                result?.OnTransact(FAILURE_REASON.INV_DATA);
                return false;
            }
            return dataSource.TryAdd(converter(value), result, exec_now);
        }
        internal static bool UpdateFromSingle<T, V>(
            this IMyDataSource<T> dataSource,
            Func<V, T, int> updater,
            V? value,
            Expression<Func<T, bool>> findmatch_expr,
            DataTransResult? result,
            bool exec_now) {
            if (dataSource.Queryable == null) {
                result?.OnTransact(FAILURE_REASON.INV_SRCS);
                return false;
            }
            if (value == null) {
                result?.OnTransact(FAILURE_REASON.INV_DATA);
                return false;
            }
            List<T> updating_list = dataSource.Queryable.Where(findmatch_expr).ToList();
            if (updating_list.Count != 0) {
                foreach (T data_obj in updating_list) {
                    updater(value, data_obj);
                }
                return dataSource.TryUpdate(updating_list, result, exec_now);
            } else {
                result?.OnTransact(FAILURE_REASON.INV_DATA);
                return false;
            }
        }
    }
}
