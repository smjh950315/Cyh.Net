using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public interface IDTOHelper<T>
    {
        IDataSourceActivator Activator { get; set; }
        IMyDataSource<T>? DataSource { get; set; }
    }
    public interface IDTOHelper<T, V> : IDTOHelper<T>, IDataRepository<V>
    {
        Expression<Func<T, V>> GetExprToView(T? x = default);
        Expression<Func<V, T>> GetExprToData(V? x = default);
        Expression<Func<T, bool>> GetExprToFindData(V view);
        int UpdateToData(V view, T data);
    }
}
