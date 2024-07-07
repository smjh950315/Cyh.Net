using System.Linq.Expressions;

namespace Cyh.Net.Data {
    public interface IDtoHandler<T, Dto> {
        Expression<Func<T, Dto>> ExprToDTO { get; set; }
        Expression<Func<Dto, T>> ExprFromDTO { get; set; }
        IDataTransResult ReadException(Exception? exception);
    }
}
