using Cyh.Net.Data.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public interface IDataRepository<T>
    {
        /// <summary>
        /// Determines whether exist an element satisfies the input condition.
        /// </summary>
        /// <param name="predicate">condition</param>
        /// <returns>
        /// Return whether exist an element satisfies the input condition
        /// <para>If no condition input, return whether any element exist.</para>
        /// </returns>
        bool Any(Expression<Func<T, bool>>? predicate, DataTransResult? result);

        /// <summary>
        /// Get count of element in the sequence that satisfies the input condition.
        /// </summary>
        /// <param name="predicate">condition</param>
        /// <returns>
        /// Retuen a number that represents the count of elements in the sequence that satisfy the input condition.
        /// <para>If no condition input, return a number represents the count of elements in sequence.</para>
        /// </returns>
        int Count(Expression<Func<T, bool>>? predicate, DataTransResult? result);

        /// <summary>
        /// Get first element in the sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate">condition</param>
        /// <param name="result"></param>
        /// <returns>
        /// First element in the sequence that satisfy the condition or default if no element satisfied the condition.
        /// <para>If no condition input, return the first element (or defualt if no elements) in the sequence.</para>
        /// </returns>
        T? GetFirst(Expression<Func<T, bool>>? predicate, DataTransResult? result);

        /// <summary>
        /// Get elements in sequence that satisfy the input condition with specific count.
        /// </summary>
        /// <param name="predicate">condition</param>
        /// <param name="dataRange"></param>
        /// <param name="result"></param>
        /// <returns>
        /// A collection of elements that satisfied the condition in the sequence.
        /// <para>If no condition input, return a collection without checked the condition.</para>
        /// <para>*Warning! If no range input, return all satisfied elements.</para>
        /// </returns>
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate, DataRange? dataRange, DataTransResult? result);

        /// <summary>
        /// Get elements in sequence that satisfy the input condition with specific count and order.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="predicate">condition</param>
        /// <param name="order">order function</param>
        /// <param name="dataRange"></param>
        /// <param name="result"></param>
        /// <returns>
        /// A collection of elements that satisfied the condition in the sequence.
        /// <para>If no condition or order input, return a collection without checked the condition or order.</para>
        /// <para>*Warning! If no range input, return all satisfied elements.</para>
        /// </returns>
        IEnumerable<T> GetAllByAsc<TKey>(Expression<Func<T, bool>>? predicate, Expression<Func<T, TKey>>? order, DataRange? dataRange, DataTransResult? result);

        /// <summary>
        /// Get elements in sequence that satisfy the input condition with specific count and order.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="predicate">condition</param>
        /// <param name="order">order function</param>
        /// <param name="dataRange"></param>
        /// <param name="result"></param>
        /// <returns>
        /// A collection of elements that satisfied the condition in the sequence.
        /// <para>If no condition or order input, return a collection without checked the condition or order.</para>
        /// <para>*Warning! If no range input, return all satisfied elements.</para>
        /// </returns>
        IEnumerable<T> GetAllByDesc<TKey>(Expression<Func<T, bool>>? predicate, Expression<Func<T, TKey>>? order, DataRange? dataRange, DataTransResult? result);

        /// <summary>
        /// Add an element into data sets.
        /// </summary>
        /// <returns></returns>
        DataTransResult Add(T? data, DataTransResult? result, bool exec_now);

        /// <summary>
        /// Update an element exist in data sets.
        /// </summary>
        /// <returns></returns>
        DataTransResult Update(T? data, DataTransResult? result, bool exec_now);
    }
}
