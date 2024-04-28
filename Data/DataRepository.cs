using Cyh.Net.Data.Extension;
using Cyh.Net.Data.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data
{
    public class DataRepository
    {
        public static IDataRepository<V>? GetDataRepository<T, V>(DTOArguments<T, V> args) {
            Lib.ThrowNull(args, args.Activator);
            Lib.ThrowNull(args.Activator, args.ExprConvertToView, args.ExprConvertToData, args.CallbackUpdateData);
            DataRepository<T, V>._Callback_UpdateData ??= args.CallbackUpdateData;
            DataRepository<T, V>._Expr_ConvertToData ??= args.ExprConvertToData;
            DataRepository<T, V>._Expr_ConvertToView ??= args.ExprConvertToView;
            DataRepository<T, V>._Callback_GetExprFindData ??= args.CallbackGetExprToFindData;
            return new DataRepository<T, V>(args.Activator);
        }
    }
    public class DataRepository<TData, TView> : IDTOHelper<TData, TView>
    {
        internal static Expression<Func<TData, TView>>? _Expr_ConvertToView;
        internal static Expression<Func<TView, TData>>? _Expr_ConvertToData;
        internal static Func<TView, Expression<Func<TData, bool>>>? _Callback_GetExprFindData;
        internal static Func<TView, TData, int>? _Callback_UpdateData;
        public IDataSourceActivator Activator { get; set; }
        public IMyDataSource<TData>? DataSource { get; set; }
        internal DataRepository(IDataSourceActivator activator) { this.Activator = activator; }

        public bool Any(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.HasAny(predicate);
        }

        public int Count(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.GetCount(predicate);
        }

        public TView? GetFirst(Expression<Func<TView, bool>>? predicate, DataTransResult? result) {
            return this.TryGetFirst(predicate, result);
        }

        public IEnumerable<TView> GetAll(Expression<Func<TView, bool>>? predicate, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAll(predicate, dataRange, result);
        }

        public IEnumerable<TView> GetAllByAsc<TKey>(Expression<Func<TView, bool>>? predicate, Expression<Func<TView, TKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAllByAsc(predicate, order, dataRange, result);
        }

        public IEnumerable<TView> GetAllByDesc<TKey>(Expression<Func<TView, bool>>? predicate, Expression<Func<TView, TKey>>? order, DataRange? dataRange, DataTransResult? result) {
            return this.TryGetAllByDesc(predicate, order, dataRange, result);
        }
        public DataTransResult Add(TView? data, DataTransResult? result, bool exec_now) {
            return this.TryAdd(data, result, exec_now);
        }
        public DataTransResult Update(TView? data, DataTransResult? result, bool exec_now) {
            return this.TryUpdate(data, result, exec_now);
        }

        public Expression<Func<TView, TData>> GetExprToData(TView? x = default) {
            Lib.ThrowNull(_Expr_ConvertToData);
            return _Expr_ConvertToData;
        }

        public Expression<Func<TData, bool>> GetExprToFindData(TView view) {
            Lib.ThrowNull(_Callback_GetExprFindData);
            return _Callback_GetExprFindData(view);
        }

        public Expression<Func<TData, TView>> GetExprToView(TData? x = default) {
            Lib.ThrowNull(_Expr_ConvertToView);
            return _Expr_ConvertToView;
        }

        public int UpdateToData(TView view, TData data) {
            Lib.ThrowNull(_Callback_UpdateData);
            return _Callback_UpdateData(view, data);
        }
    }
}