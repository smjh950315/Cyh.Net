namespace Cyh.Net.Data
{
    public interface IDataSourceActivator
    {
        IMyDataSource<T> GetDataSource<T>();
    }
}
