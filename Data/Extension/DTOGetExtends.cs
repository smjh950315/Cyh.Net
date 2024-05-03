using Cyh.Net.Data.Internal;
using Cyh.Net.Data.Models;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Cyh.Net.Data.Extension {
    public static class DTOGetExtends {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryActivate<T>(this IDTOHelper<T> dto, [NotNullWhen(true)] out IMyDataSource<T>? dataSource) {
            if (dto.DataSource == null) {
                dto.DataSource = dto.Activator.GetDataSource<T>();
                if (dto.DataSource == null) {
                    dataSource = null;
                    return false;
                }
            }
            dataSource = dto.DataSource;
            return true;
        }

        /// <summary>
        /// Try to get the first data from the data source.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <param name="result"></param>
        /// <returns>Satisfied data</returns>
        public static V? TryGetFirst<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, DataTransResult? result) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return default;
            }
            return dataSource.GetSingle(dto.GetExprToView(), filter, result);
        }

        /// <summary>
        /// Try to get all data from the data source.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <param name="dataRange">The range to get data.</param>
        /// <param name="result"></param>
        /// <returns>Satisfied data collection.</returns>
        public static IEnumerable<V> TryGetAll<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, DataRange? dataRange, DataTransResult? result) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return Enumerable.Empty<V>();
            }
            return dataSource.GetMultiple(dto.GetExprToView(), filter, dataRange, result);
        }

        /// <summary>
        /// Try to get all data from the data source by ascending order.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <typeparam name="VKey"></typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <param name="order"></param>
        /// <param name="dataRange">The range to get data.</param>
        /// <param name="result"></param>
        /// <returns>Satisfied data collection.</returns>
        public static IEnumerable<V> TryGetAllByAsc<T, V, VKey>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, Expression<Func<V, VKey>>? order, DataRange? dataRange, DataTransResult? result) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return Enumerable.Empty<V>();
            }
            return dataSource.GetMultipleAsc(dto.GetExprToView(), filter, order, dataRange, result);
        }

        /// <summary>
        /// Try to get all data from the data source by descending order.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <typeparam name="VKey"></typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <param name="order"></param>
        /// <param name="dataRange">The range to get data.</param>
        /// <param name="result"></param>
        /// <returns>Satisfied data collection.</returns>
        public static IEnumerable<V> TryGetAllByDesc<T, V, VKey>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter, Expression<Func<V, VKey>>? order, DataRange? dataRange, DataTransResult? result) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return Enumerable.Empty<V>();
            }
            return dataSource.GetMultipleDesc(dto.GetExprToView(), filter, order, dataRange, result);
        }

        /// <summary>
        /// Inidicate whether the data source has any data.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <returns>Whether exist satisfied data.</returns>
        public static bool HasAny<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return false;
            }
            return dataSource.HasAny(dto.GetExprToView(), filter);
        }

        /// <summary>
        /// Try to get the count of data from the data source.
        /// </summary>
        /// <typeparam name="T">Inner data type.</typeparam>
        /// <typeparam name="V">Extern data type.</typeparam>
        /// <param name="filter">Expression to filter the data.</param>
        /// <returns>Count of satisfied data.</returns>
        public static int GetCount<T, V>(this IDTOHelper<T, V> dto, Expression<Func<V, bool>>? filter) {
            if (!TryActivate(dto, out IMyDataSource<T>? dataSource)) {
                return 0;
            }
            return dataSource.GetCount(dto.GetExprToView(), filter);
        }
    }
}
