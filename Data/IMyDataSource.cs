using Cyh.Net.Data.Models;

namespace Cyh.Net.Data
{
    public interface IMyDataSource
    {
        bool TryAdd(object? @object, DataTransResult? transResult, bool exec_now);

        bool TryUpdate(object? @object, DataTransResult? transResult, bool exec_now);

        bool ApplyChanges(DataTransResult? transResult);
    }
    public interface IMyDataSource<T> : IMyDataSource
    {
        IQueryable<T>? Queryable { get; }

        /// <summary>
        /// Try add new data.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="transResult"></param>
        /// <param name="exec_now"></param>
        /// <returns>Return whether succeed without error.</returns>
        bool TryAdd(T? @object, DataTransResult? transResult, bool exec_now);

        /// <summary>
        /// Try update exist data
        /// </summary>
        /// <param name="object"></param>
        /// <param name="transResult"></param>
        /// <param name="exec_now"></param>
        /// <returns>Return whether succeed without error.</returns>
        bool TryUpdate(T? @object, DataTransResult? transResult, bool exec_now);

        /// <summary>
        /// Try update exist data
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="transResult"></param>
        /// <param name="exec_now"></param>
        /// <returns>Return whether succeed without error.</returns>
        bool TryUpdate(IEnumerable<T> @objects, DataTransResult? transResult, bool exec_now);
    }
}