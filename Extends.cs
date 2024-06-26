using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

        /// <summary>
        /// Encode the string into bytes in utf-8 encoding
        /// </summary>
        /// <returns>Bytes result of string in Utf-8 encoding</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetBytesUtf8(this string? str) => !str.IsNullOrEmpty() ? Encoding.UTF8.GetBytes(str) : Array.Empty<byte>();

        /// <summary>
        /// Decode byte data to string in Utf-8 encoding
        /// </summary>
        /// <returns>String result decoded from bytes in Utf-8 encoding</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringAsUtf8(this byte[]? bytes) => !bytes.IsNullOrEmpty() ? Encoding.UTF8.GetString(bytes) : String.Empty;

        /// <summary>
        /// Serialize object model into json string
        /// </summary>
        /// <returns>Json string from serialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? Serialize<T>(this T? value, JsonSerializerOptions? options = null)
            => value != null ? JsonSerializer.Serialize(value, options) : null;

        /// <summary>
        /// Deserialize the string into object model
        /// </summary>
        /// <returns>Object model from deserialized string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Deserialize<T>(this string? str, JsonSerializerOptions? options = null)
            => !str.IsNullOrEmpty() ? JsonSerializer.Deserialize<T>(str, options) : default;

        /// <summary>
        /// Serialize object model into json string
        /// </summary>
        /// <param name="output">Json string from serialized object if completed without error, otherwise null</param>
        /// <returns>Whether serialized without error</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerialize<T>(this T? value, [NotNullWhen(true)] out string? output, JsonSerializerOptions? options = null) {
            try {
                output = value.Serialize(options);
                return !output.IsNullOrEmpty();
            } catch {
                output = null;
                return false;
            }
        }

        /// <summary>
        /// Deserialize the string into object model
        /// </summary>
        /// <param name="output">Object model from deserialized string if completed without error, otherwise null</param>
        /// <returns>Whether deserialized without error</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDeserialize<T>(this string? str, [NotNullWhen(true)] out T? output, JsonSerializerOptions? options = null) {
            try {
                output = str.Deserialize<T>(options);
                return output != null;
            } catch {
                output = default;
                return false;
            }
        }

        /// <summary>
        /// Serialize object model into utf-8 json string
        /// </summary>
        /// <returns>Utf-8 son string from serialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[]? SerializeAsUtf8<T>(this T? value, JsonSerializerOptions? options = null)
            => value.Serialize(options).GetBytesUtf8();

        /// <summary>
        /// Deserialize the utf-8 json string into object model
        /// </summary>
        /// <returns>Object model from deserialized utf-8 json string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? DeserializeAsUtf8<T>(this byte[]? bytes, JsonSerializerOptions? options = null)
            => bytes.GetStringAsUtf8().Deserialize<T>(options);

        /// <summary>
        /// Compare a collection to another
        /// </summary>
        /// <returns>True if element is equal in both collection</returns>
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

        /// <summary>
        /// Whether the input type is number
        /// </summary>
        /// <returns>True if the input type is number, otherwise false</returns>
        public static bool IsNumeric(this Type type) {
            if (type == null) { return false; }
            return type.IsAnyOf(Typedef.Numeric);
        }

        /// <summary>
        /// Whether the input type is struct
        /// </summary>
        /// <returns>True if the input type is struct, otherwise false</returns>
        public static bool IsStruct(this Type type) {
            return type.IsValueType || type.IsEnum;
        }

        /// <summary>
        /// Whether the input type is unmanaged, and excluding any managed members
        /// </summary>
        /// <returns>True if the input type is unmanaged, otherwise false</returns>
        public static bool IsUnmanaged(this Type type) {
            if (type.IsPrimitive || type.IsPointer || type.IsEnum) {
                return true;
            } else if (type.IsValueType) {
                var members = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var member in members) {
                    if (!IsUnmanaged(member.FieldType)) return false;
                }
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Whether the input type contain the StructureLayout attribute
        /// </summary>
        /// <returns>True if the type input contain the StructureLayout attribute, otherwise false</returns>
        public static bool HasStructureLayout(this Type type) {
            try {
                if (!IsStruct(type)) {
                    return false;
                } else {
                    return type.StructLayoutAttribute != null;
                }
            } catch {
                return false;
            }
        }
    }
}
