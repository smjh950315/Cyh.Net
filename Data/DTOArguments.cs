using System.Linq.Expressions;

namespace Cyh.Net.Data {

    /// <summary>
    /// Arguments for DTO to create a data repository
    /// </summary>
    /// <typeparam name="T">Original data type</typeparam>
    /// <typeparam name="V">External data type</typeparam>
    public class DTOArguments<T, V> {

        /// <summary>
        /// DataSource generator
        /// </summary>
        public IDataSourceGenerator Activator { get; set; } = null!;

        /// <summary>
        /// Expression to convert data to view
        /// </summary>
        public Expression<Func<T, V>> ExprConvertToView { get; set; } = null!;

        /// <summary>
        /// Expression to convert view to data
        /// </summary>
        public Expression<Func<V, T>> ExprConvertToData { get; set; } = null!;

        /// <summary>
        /// Callback to update data
        /// </summary>
        public Func<V, T, int> CallbackUpdateData { get; set; } = null!;

        /// <summary>
        /// Callback to get expression to find the related data
        /// </summary>
        public Func<V, Expression<Func<T, bool>>> CallbackGetExprToFindData { get; set; } = null!;
    }
}
