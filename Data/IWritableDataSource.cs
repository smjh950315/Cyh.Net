using System.Diagnostics.CodeAnalysis;

namespace Cyh.Net.Data {
    public interface IWritableDataSource {

    }

    public interface IWritableDataSource<T> : IWritableDataSource {
        void Add(T value);
        void Update(T value);
        void Add(IEnumerable<T> value);
        void Update(IEnumerable<T> value);
        bool Save([NotNullWhen(false)] out Exception? internalException);
    }
}
