using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Cyh.Net.DependencyInjection;
using Cyh.Net.Internal;

namespace Cyh.Net
{
    public static class Extends
    {
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
            var itSelf = values.GetEnumerator();
            var itOther = others.GetEnumerator();
            while (itSelf.MoveNext() && itOther.MoveNext())
            {
                if (!itSelf.Current.Equals(itOther.Current)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Whether the input type is number
        /// </summary>
        /// <returns>True if the input type is number, otherwise false</returns>
        public static bool IsNumeric(this Type type)
        {
            if (type == null) { return false; }
            return type.IsAnyOf(Typedef.Numeric);
        }

        /// <summary>
        /// Whether the input type is struct
        /// </summary>
        /// <returns>True if the input type is struct, otherwise false</returns>
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType || type.IsEnum;
        }

        /// <summary>
        /// Whether the input type is unmanaged, and excluding any managed members
        /// </summary>
        /// <returns>True if the input type is unmanaged, otherwise false</returns>
        public static bool IsUnmanaged(this Type type)
        {
            if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            {
                return true;
            }
            else if (type.IsValueType)
            {
                var members = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var member in members)
                {
                    if (!IsUnmanaged(member.FieldType)) return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Whether the input type contain the StructureLayout attribute
        /// </summary>
        /// <returns>True if the type input contain the StructureLayout attribute, otherwise false</returns>
        public static bool HasStructureLayout(this Type type)
        {
            try
            {
                if (!IsStruct(type))
                {
                    return false;
                }
                else
                {
                    return type.StructLayoutAttribute != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Indicate whether the type is Nullable`T`
        /// </summary>
        /// <returns>True if the type is Nullable`T`, otherwise false</returns>
        public static bool IsNullableWrapped(this Type type)
        {
            return type.Name.StartsWith("Nullable");
        }

        /// <summary>
        /// Get type if not null
        /// </summary>
        /// <returns>Type of Nullable`T`.Value or current type if not Nullable`T`</returns>
        public static Type GetNotNullType(this Type type)
        {
            if (type.IsNullableWrapped())
            {
#pragma warning disable CS8602
                return type.GetProperty("Value").PropertyType;
#pragma warning restore CS8602
            }
            else
            {
                return type;
            }
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

        /// <summary>
        /// Indicate whether current type is dereived from <paramref name="parent"/>
        /// </summary>
        /// <param name="parent">Type of parent object</param>
        /// <returns>True if current type is child of <paramref name="parent"/>, otherwise false</returns>
        public static bool IsChildOf(this Type type, Type parent)
        {
            return type.IsSubclassOf(parent);
        }

        /// <summary>
        /// Indicate whether current type is dereived from <typeparamref name="T"/>
        /// </summary>
        /// <returns>True if current type is child of <typeparamref name="T"/>, otherwise false</returns>
        public static bool IsChildOf<T>(this Type type)
        {
            return type.IsChildOf(typeof(T));
        }

        /// <summary>
        /// Indicate whether <paramref name="parent"/> is dereived from current type
        /// </summary>
        /// <param name="parent">Type of parent object</param>
        /// <returns>True if current type is child of <paramref name="parent"/>, otherwise false</returns>
        public static bool IsParentOf(this Type type, Type child)
        {
            return child.IsSubclassOf(type);
        }

        /// <summary>
        /// Indicate whether current type dereived from <typeparamref name="T"/>
        /// </summary>
        /// <returns>True if current type is child of <typeparamref name="T"/>, otherwise false</returns>
        public static bool IsParentOf<T>(this Type type)
        {
            return type.IsParentOf(typeof(T));
        }

        /// <summary>
        /// Make instance of <paramref name="type"/> by <paramref name="constructArgs"/> as constructor parameters
        /// </summary>
        /// <param name="type">Dest type</param>
        /// <param name="constructArgs">constructor parameters</param>
        /// <returns>Instance of <paramref name="type"/> or null if failure or no matched constructor</returns>
        public static object? Construct(this Type type, params object[] constructArgs)
        {
            try
            {
                Type[] constructorTypes = new Type[constructArgs.Length];
                for (int i = 0; i < constructorTypes.Length; i++)
                {
                    constructorTypes[i] = constructArgs[i].GetType();
                }
                var constructor = type.GetConstructor(constructorTypes);
                if (constructor == null) { return null; }
                return constructor.Invoke(constructArgs);
            }
            catch (Exception e)
            {
                e.Print();
                return null;
            }
        }

        /// <summary>
        /// Make instance of <paramref name="type"/> by <paramref name="constructArgs"/> as constructor parameters
        /// </summary>
        /// <param name="type">Dest type</param>
        /// <param name="constructArgs">constructor parameters</param>
        /// <returns>Instance of <paramref name="type"/> or null if failure or no matched constructor</returns>
        public static bool Construct(this Type type, [NotNullWhen(true)] out object? instance, params object[] constructArgs)
        {
            instance = type.Construct(constructArgs);
            return instance != null;
        }

        /// <summary>
        /// Get registered service <typeparamref name="T"/> from service provider
        /// </summary>
        /// <typeparam name="T">Type of service to get</typeparam>
        /// <param name="serviceInstance">The instance of service</param>
        /// <returns>True if service if found, otherwise false</returns>
        public static bool GetInjectedService<T>(this IServiceProvider serviceProvider, [NotNullWhen(true)] out T? serviceInstance) where T : class
        {
            serviceInstance = serviceProvider.GetService(typeof(T)) as T;
            return serviceInstance != null;
        }

        /// <summary>
        /// Inject the service <typeparamref name="T"/> if <typeparamref name="T"/> is not found in service provider
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="implFactory">The implement of service factory</param>
        public static void InjectScopedIfNotExisting<T>(this IDependencyInjectionData diData, Func<IServiceProvider, T> implFactory) where T : class
        {
            object? service = diData.GetService(typeof(T));
            if (service == null)
            {
                diData.AddScoped(typeof(T), implFactory);
            }
        }

        /// <summary>
        /// Inject the service <typeparamref name="T"/> if <typeparamref name="T"/> is not found in service provider
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="factoryMethod">The implement of service factory</param>
        public static void InjectScopedIfNotExisting<T>(this IDependencyInjectionData diService, Func<T?> factoryMethod) where T : class
        {
            var implFactory = Lib.MakeServiceFactory(factoryMethod);
            InjectScopedIfNotExisting(diService, implFactory);
        }

        public static Expression<Func<T, bool>> UpdateExpression<T>(this Expression<Func<T, bool>>? originalExpression, Expression<Func<T, bool>> additionalExpression, bool _and)
        {
            if (originalExpression == null)
            {
                return additionalExpression;
            }
            // get the visitor
            var visitor = new ParameterUpdateVisitor(originalExpression.Parameters.First(), additionalExpression.Parameters.First());
            // replace the parameter in the original expression
            originalExpression = visitor.Visit(originalExpression) as Expression<Func<T, bool>>;
            // now you can and together the two expressions
            BinaryExpression? binExp;
            if (_and)
            {
                binExp = Expression.And(additionalExpression.Body, originalExpression.Body);
            }
            else
            {
                binExp = Expression.Or(additionalExpression.Body, originalExpression.Body);
            }
            // and return a new lambda, that will do what you want.
            // NOTE that the binExp has reference only to te newExp.Parameters[0] (there is only 1) parameter, and no other
            return Expression.Lambda<Func<T, bool>>(binExp, originalExpression.Parameters);
        }

        public static T[] GetEnumArray<T>(this int[]? ints) where T : Enum
        {
            if (ints.IsNullOrEmpty()) return Array.Empty<T>();
            T[] results = new T[ints.Length];
            for (int i = 0; i < ints.Length; i++)
            {
                results[i] = (T)(object)ints[i];
            }
            return results;
        }
    }
}
