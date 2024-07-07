namespace Cyh.Net.Data {
    public interface IDataSource<T> : IReadOnlyDataSource<T>, IWritableDataSource<T> { }
}
