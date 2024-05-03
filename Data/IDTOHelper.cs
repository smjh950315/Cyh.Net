using System.Linq.Expressions;

namespace Cyh.Net.Data {

    /// <summary>
    /// The DTO Helper Interface of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Inner data type</typeparam>
    public interface IDTOHelper<T> {

        /// <summary>
        /// The data source generator
        /// </summary>
        IDataSourceGenerator Activator { get; set; }

        /// <summary>
        /// Inner data source
        /// </summary>
        IMyDataSource<T>? DataSource { get; set; }
    }

    /// <summary>
    /// The DTO Helper Interface of <typeparamref name="T"/> and <typeparamref name="V"/>
    /// </summary>
    /// <typeparam name="T">Inner data type</typeparam>
    /// <typeparam name="V">Extern data type</typeparam>
    public interface IDTOHelper<T, V> : IDTOHelper<T>, IDataRepository<V> {

        /// <summary>
        /// Get the expression to convert data to view
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        Expression<Func<T, V>> GetExprToView(T? x = default);

        /// <summary>
        /// Get the expression to convert view to data
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        Expression<Func<V, T>> GetExprToData(V? x = default);

        /// <summary>
        /// Get the expression to find data from view
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Expression<Func<T, bool>> GetExprToFindData(V view);

        /// <summary>
        /// Update inner data from extern data
        /// </summary>
        int UpdateToData(V view, T data);
    }
}
