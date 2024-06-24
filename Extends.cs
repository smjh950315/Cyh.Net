using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Cyh.Net {
    public static class Extends {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[]? GetBytesUtf8(this string? str) => !str.IsNullOrEmpty() ? Encoding.UTF8.GetBytes(str) : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetStringAsUtf8(this byte[]? bytes) => !bytes.IsNullOrEmpty() ? Encoding.UTF8.GetString(bytes) : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? Serialize<T>(this T? value, JsonSerializerOptions? options = null)
            => value != null ? JsonSerializer.Serialize(value, options) : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Deserialize<T>(this string? str, JsonSerializerOptions? options = null)
            => !str.IsNullOrEmpty() ? JsonSerializer.Deserialize<T>(str, options) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerialize<T>(this T? value, out string? output, JsonSerializerOptions? options = null) {
            try {
                output = value.Serialize(options);
                return !output.IsNullOrEmpty();
            } catch {
                output = null;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDeserialize<T>(this string? str, out T? output, JsonSerializerOptions? options = null) {
            try {
                output = str.Deserialize<T>(options);
                return output != null;
            } catch {
                output = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[]? SerializeAsUtf8<T>(this T? value, JsonSerializerOptions? options = null)
            => value.Serialize(options).GetBytesUtf8();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? DeserializeAsUtf8<T>(this byte[]? bytes, JsonSerializerOptions? options = null)
            => bytes.GetStringAsUtf8().Deserialize<T>(options);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqualTo<T>(this IEnumerable<T>? values, IEnumerable<T>? others) where T : IEquatable<T> {
            if (values.IsNullOrEmpty() || others.IsNullOrEmpty()) { return false; }
            if (values.Count() != others.Count()) { return false; }
            var itSelf = values.GetEnumerator();
            var itOther = others.GetEnumerator();
            while (itSelf.MoveNext() && itOther.MoveNext()) {
                if (!itSelf.Current.Equals(itOther.Current)) { return false; }
            }
            return true;
        }
    }
}
