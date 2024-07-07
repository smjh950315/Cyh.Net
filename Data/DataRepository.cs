using Cyh.Net.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data {
    public class DataRepository {
        public static IDataRepository<Dto> GetDataRepository<T, Dto>(IDataSource<T> dataSource, IDtoHandler<T, Dto> dtoHandler) {
            return new DataRepositoryImpl<T, Dto>(dataSource, dtoHandler);
        }
    }

    internal class DataRepositoryImpl<T, Dto> : IDataRepository<Dto> {
        internal static void s_DoAdd(IWritableDataSource<T> writeAccesser, T data) => writeAccesser.Add(data);
        internal static void s_DoUpdate(IWritableDataSource<T> writeAccesser, T data) => writeAccesser.Update(data);
        internal static void s_DoBatchAdd(IWritableDataSource<T> writeAccesser, IEnumerable<T> datas) => writeAccesser.Add(datas);
        internal static void s_DoBatchUpdate(IWritableDataSource<T> writeAccesser, IEnumerable<T> datas) => writeAccesser.Update(datas);
        internal static bool s_DoSave(IWritableDataSource<T> writeAccesser, out Exception? internalException) => writeAccesser.Save(out internalException);
        internal static T s_MakeData(IDtoHandler<T, Dto> handler, Dto dto) {
            return handler.ExprFromDTO.Compile()(dto);
        }
        internal static IEnumerable<T> s_BatchMakeData(IDtoHandler<T, Dto> handler, IEnumerable<Dto> dtos) {
            if (dtos is IQueryable<Dto> qdto) {
                return qdto.Select(handler.ExprFromDTO);
            } else {
                return dtos.Select(handler.ExprFromDTO.Compile());
            }
        }
        internal unsafe static void s_Execute<TDest, TConvertSrc, TDestData, TSrcData>(
            delegate*<TDest, TDestData, void> fnDoAction,
            delegate*<TDest, out Exception?, bool> fnDoSave,
            delegate*<TConvertSrc, TSrcData, TDestData> fnConvertData,
            TConvertSrc? convertSrc, TDest? destination, TSrcData? srcData,
            IDataTransResult? result, bool save_immediately) {

            if (convertSrc == null || destination == null) {
                result.TrySetResCode(ResultEnum.BadSourceError); return;
            }
            if (srcData != null) {
                try {
                    var data = fnConvertData(convertSrc, srcData);
                    fnDoAction(destination, data);
                } catch (Exception ex) {
                    result.TrySetResCode(ResultEnum.DataTransferError, ex.Message);
                }
            }
            if (!save_immediately) { return; }
            if (!fnDoSave(destination, out Exception? internalException)) {
                result.TrySetResCode(ResultEnum.InternalError, internalException?.Message);
            }
        }

        IDtoHandler<T, Dto> m_dtoHandler;
        IDataSource<T> m_dataSource;

        public DataRepositoryImpl(IDataSource<T> dataSource, IDtoHandler<T, Dto> dtoHandler) {
            this.m_dtoHandler = dtoHandler;
            this.m_dataSource = dataSource;
        }

        public void Add(Dto? dto, IDataTransResult? result, bool save_immediately) {
            unsafe {
                s_Execute(&s_DoAdd, &s_DoSave, &s_MakeData, this.m_dtoHandler, this.m_dataSource, dto, result, save_immediately);
            }
        }
        public void Add(IEnumerable<Dto> dtos, IDataTransResult? result, bool save_immediately) {
            unsafe {
                s_Execute(&s_DoBatchAdd, &s_DoSave, &s_BatchMakeData, this.m_dtoHandler, this.m_dataSource, dtos, result, save_immediately);
            }
        }
        public void Update(Dto? dto, IDataTransResult? result, bool save_immediately) {
            unsafe {
                s_Execute(&s_DoUpdate, &s_DoSave, &s_MakeData, this.m_dtoHandler, this.m_dataSource, dto, result, save_immediately);
            }
        }
        public void Update(IEnumerable<Dto> dtos, IDataTransResult? result, bool save_immediately) {
            unsafe {
                s_Execute(&s_DoBatchUpdate, &s_DoSave, &s_BatchMakeData, this.m_dtoHandler, this.m_dataSource, dtos, result, save_immediately);
            }
        }

        public IEnumerable<Dto> GetAll(Expression<Func<Dto, bool>>? filter, DataRange? dataRange) {
            return this.m_dataSource?.ReadOnlyAccesser.TryUse(this.m_dtoHandler)?.TryUse(filter)?.TryUse(dataRange) ?? [];
        }

        public IEnumerable<Dto> GetAll<DtoKey>(Expression<Func<Dto, bool>>? filter, Func<Dto, DtoKey>? order, DataRange? dataRange) {
            return this.m_dataSource?.ReadOnlyAccesser.TryUse(this.m_dtoHandler)?.TryUse(filter)?.TryUse(order).TryUse(dataRange) ?? [];
        }

        public Dto? GetFirst(Expression<Func<Dto, bool>>? filter) {
            return this.m_dtoHandler.CanRead(this.m_dataSource, out IQueryable<Dto>? readAccesser) ? readAccesser.TryUse(filter).FirstOrDefault() : default;
        }

        public Dto? GetFirst<DtoKey>(Expression<Func<Dto, bool>>? filter, Func<Dto, DtoKey>? order) {
            return this.m_dtoHandler.CanRead(this.m_dataSource, out IQueryable<Dto>? readAccesser) ? readAccesser.TryUse(filter).TryUse(order).FirstOrDefault() : default;
        }
    }
}
