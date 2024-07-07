namespace Cyh.Net.Data {
    public class DataSourceBase<T, P> : IReadOnlyDataSource<T> {

        public IQueryable<T>? ReadOnlyAccesser { get; set; }

    }
}
