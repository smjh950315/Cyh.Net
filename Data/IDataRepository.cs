using Cyh.Net.Models;
using System.Linq.Expressions;

namespace Cyh.Net.Data {
    public interface IDataRepository<Dto> {
        Dto? GetFirst(Expression<Func<Dto, bool>>? filter);
        Dto? GetFirst<DtoKey>(Expression<Func<Dto, bool>>? filter, Func<Dto, DtoKey>? order);
        IEnumerable<Dto> GetAll(Expression<Func<Dto, bool>>? filter, DataRange? dataRange);
        IEnumerable<Dto> GetAll<DtoKey>(Expression<Func<Dto, bool>>? filter, Func<Dto, DtoKey>? order, DataRange? dataRange);
        void Add(Dto? dto, IDataTransResult? result, bool save_immediately);
        void Update(Dto? dto, IDataTransResult? result, bool save_immediately);
        void Add(IEnumerable<Dto> dtos, IDataTransResult? result, bool save_immediately);
        void Update(IEnumerable<Dto> dtos, IDataTransResult? result, bool save_immediately);
    }
}
