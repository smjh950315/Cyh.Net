using System.Linq.Expressions;

namespace Cyh.Net.Data {
    public interface IDtoHandler<T, Dto> {
        Expression<Func<T, Dto>> ExprToDTO { get; }
        Expression<Func<Dto, T>> ExprFromDTO { get; }
        IDataTransResult ReadException(Exception? exception);
    }
}
