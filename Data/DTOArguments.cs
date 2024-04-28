using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public class DTOArguments<T, V>
    {
        public IDataSourceActivator Activator { get; set; } = null!;
        public Expression<Func<T, V>> ExprConvertToView { get; set; } = null!;
        public Expression<Func<V, T>> ExprConvertToData { get; set; } = null!;
        public Func<V, T, int> CallbackUpdateData { get; set; } = null!;
        public Func<V, Expression<Func<T, bool>>> CallbackGetExprToFindData { get; set; } = null!;
    }
}
