using Cyh.Net.Reflection;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Cyh.Net;

public static class Extensions
{
    private delegate bool TryParse_<T>(string? value, out T result);
    private static Dictionary<Type, object?> _cachedTryParseMethods = [];

    private static object? GetTryParseMethod(this Type type)
    {
        if (!type.IsStruct())
            return null;
        if (_cachedTryParseMethods.TryGetValue(type, out object? result))
            return result;

        MethodInfo? method = type.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, [typeof(string), type.MakeByRefType()]);
        if (method == null)
        {
            _cachedTryParseMethods[type] = null;
            return null;
        }
        Delegate _delegate = method.CreateDelegate(typeof(TryParse_<>).MakeGenericType([type]));
        _cachedTryParseMethods.Add(type, _delegate);
        return _delegate;
    }

    private static TryParse_<T>? GetTryParseMethod<T>() where T : struct
    {
        object? result = GetTryParseMethod(typeof(T));
        if (result is TryParse_<T> _delegate)
            return _delegate;
        return null;
    }

    /// <summary>
    /// Determines whether the specified value is null or empty.
    /// </summary>
    /// <returns>Return true if the input sequence is null or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this IEnumerable? values)
    {
        return values == null || !values.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// Determines whether the specified value is any of the specified values.
    /// </summary>
    /// <returns>Return true if the sequence contains current value, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAnyOf<T>(this T value, IEnumerable<T> values)
    {
        return values.Contains(value);
    }

    /// <summary>
    /// Get enum[] from int[]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ints"></param>
    /// <param name="all_if_null"></param>
    /// <returns></returns>
    public static T[] GetEnumArray<T>(this int[]? ints, bool all_if_null = false) where T : Enum
    {
        if (ints == null)
        {
            if (all_if_null)
            {
                return (T[])Enum.GetValues(typeof(T));
            }
            else
            {
                return Array.Empty<T>();
            }
        }
        if (ints.Length == 0)
        {
            return Array.Empty<T>();
        }
        T[] results = new T[ints.Length];
        for (int i = 0; i < ints.Length; i++)
        {
            results[i] = (T)(object)ints[i];
        }
        return results;
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
    public static bool TrySerialize<T>(this T? value, [NotNullWhen(true)] out string? output, JsonSerializerOptions? options = null)
    {
        try
        {
            output = value.Serialize(options);
            return !output.IsNullOrEmpty();
        }
        catch
        {
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
    public static bool TryDeserialize<T>(this string? str, [NotNullWhen(true)] out T? output, JsonSerializerOptions? options = null)
    {
        try
        {
            output = str.Deserialize<T>(options);
            return output != null;
        }
        catch
        {
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
    public static bool IsEqualTo<T>(this IEnumerable<T>? values, IEnumerable<T>? others) where T : IEquatable<T>
    {
        if (values.IsNullOrEmpty() || others.IsNullOrEmpty()) { return false; }
        if (values.Count() != others.Count()) { return false; }
        IEnumerator<T> itSelf = values.GetEnumerator();
        IEnumerator<T> itOther = others.GetEnumerator();
        while (itSelf.MoveNext() && itOther.MoveNext())
        {
            if (!itSelf.Current.Equals(itOther.Current)) { return false; }
        }
        return true;
    }

    /// <summary>
    /// Print StackTrace with title if current exception is not null, otherwise do nothing
    /// </summary>
    public static void PrintStack(this Exception? exception, Action<string?>? messageHolder = null)
    {
        if (exception == null) { return; }
        Action<string?> _messageHolder;
        if (messageHolder != null)
        {
            _messageHolder = messageHolder;
        }
        else if (Lib.gs_globalMessageHandler != null)
        {
            _messageHolder = Lib.gs_globalMessageHandler;
        }
        else
        {
            _messageHolder = Console.WriteLine;
        }
        _messageHolder("===============StackTrace===============");
        _messageHolder(exception.StackTrace);
    }

    /// <summary>
    /// Print Message with title if current exception is not null, otherwise do nothing
    /// </summary>
    public static void PrintMessage(this Exception? exception, Action<string?>? messageHolder = null)
    {
        if (exception == null) { return; }
        Action<string?> _messageHolder;
        if (messageHolder != null)
        {
            _messageHolder = messageHolder;
        }
        else if (Lib.gs_globalMessageHandler != null)
        {
            _messageHolder = Lib.gs_globalMessageHandler;
        }
        else
        {
            _messageHolder = Console.WriteLine;
        }
        _messageHolder("===============Message===============");
        _messageHolder(exception.Message);
    }

    /// <summary>
    /// Print Source with title if current exception is not null, otherwise do nothing
    /// </summary>
    public static void PrintSource(this Exception? exception, Action<string?>? messageHolder = null)
    {
        if (exception == null) { return; }
        Action<string?> _messageHolder;
        if (messageHolder != null)
        {
            _messageHolder = messageHolder;
        }
        else if (Lib.gs_globalMessageHandler != null)
        {
            _messageHolder = Lib.gs_globalMessageHandler;
        }
        else
        {
            _messageHolder = Console.WriteLine;
        }
        _messageHolder("===============Source===============");
        _messageHolder(exception.Source);
    }

    /// <summary>
    /// Print all human readable message of an exception if not null
    /// </summary>
    /// <param name="exception"></param>
    public static void Print(this Exception? exception, Action<string?>? messageHolder = null)
    {
        if (exception == null) { return; }
        exception.PrintSource(messageHolder);
        exception.PrintMessage(messageHolder);
        exception.PrintStack(messageHolder);
    }

    public static bool IsBetween<T>(this T value, T lhs, T rhs, bool acceptEqual = false) where T : struct, INumber<T>
    {
        T min = T.Min(lhs, rhs), max = T.Max(lhs, rhs);
        return acceptEqual ? min < value && value < max : min <= value && value <= max;
    }

    public static T ParseOrValue<T>(this string? input, T _default = default) where T : struct
    {
        var _delegate = GetTryParseMethod<T>();
        if (_delegate == null)
            return _default;
        return _delegate(input, out T output) ? output : _default;
    }

    public static T? ParseOrNull<T>(this string? input) where T : struct
    {
        var _delegate = GetTryParseMethod<T>();
        if (_delegate == null)
            return null;
        return _delegate(input, out T output) ? output : null;
    }

    public static T ParseOrThrow<T>(this string? input, string message = "No convert method is found !") where T : struct
    {
        var _delegate = GetTryParseMethod<T>();
        if (_delegate == null)
            throw new InvalidCastException("No convert method is found !");
        return _delegate(input, out T output) ? output : throw new InvalidCastException($"Cannot convert string to {typeof(T).FullName}!");
    }
}
