namespace Cyh.Net.Data {

    /// <summary>
    /// DataSource generator
    /// </summary>
    public interface IDataSourceGenerator {

        /// <summary>
        /// Get data source of <typeparamref name="T"/>
        /// </summary>
        IMyDataSource<T> GetDataSource<T>();
    }
}
