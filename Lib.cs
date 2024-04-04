namespace Cyh.Net
{
    public static class Lib
    {
        public static T? NoThrow<T>(Func<T> func) {
            try {
                return func();
            } catch {
                return default;
            }
        }
    }
}
