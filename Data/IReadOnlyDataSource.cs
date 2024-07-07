namespace Cyh.Net.Data {

    public interface IReadOnlyDataSource {

    }

    public interface IReadOnlyDataSource<T> : IReadOnlyDataSource {
        IQueryable<T>? ReadOnlyAccesser { get; }
    }
}
