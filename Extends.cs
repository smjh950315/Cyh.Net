using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Cyh.Net
{
    public static class Extends
    {
        public static bool IsNullOrEmpty([NotNullWhen(false)] this IEnumerable? values) {
            return values == null || !values.GetEnumerator().MoveNext();
        }
    }
}
