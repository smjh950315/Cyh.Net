using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Cyh.Net
{
    public static class Extends
    {
        /// <summary>
        /// Determines whether the specified value is null or empty.
        /// </summary>
        /// <returns>Return true if the input sequence is null or empty.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this IEnumerable? values) {
            return values == null || !values.GetEnumerator().MoveNext();
        }

        /// <summary>
        /// Determines whether the specified value is any of the specified values.
        /// </summary>
        /// <returns>Return true if the sequence contains current value, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<T>(this T value, IEnumerable<T> values) {
            return values.Contains(value);
        }
    }
}
