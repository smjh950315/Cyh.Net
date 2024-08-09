using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cyh.Net.Reflection {
    public static class ObjectHelper {

        private static object? GetValue(object? instance, MemberInfo memberInfo) {
            if (memberInfo is PropertyInfo propertyInfo) {
                return propertyInfo.GetValue(instance);
            } else if (memberInfo is FieldInfo fieldInfo) {
                return fieldInfo.GetValue(instance);
            } else {
                return null;
            }
        }
        private static void SetValue(object? instance, object? value, MemberInfo memberInfo) {
            if (memberInfo is PropertyInfo propertyInfo) {
                propertyInfo.SetValue(instance, value);
            } else if (memberInfo is FieldInfo fieldInfo) {
                fieldInfo.SetValue(instance, value);
            } else {
                throw new InvalidOperationException("nuknow member information");
            }
        }
        private static MemberInfo[] GetMemberInfos(Type type, BindingFlags bindingFlags = MemberBindingFlags.InstanceMember | MemberBindingFlags.Accessable_All) {
            return type.GetMembers(bindingFlags);
        }
        private static MemberInfo? GetMemberInfo(Type type, string name, BindingFlags bindingFlags = MemberBindingFlags.InstanceMember | MemberBindingFlags.Accessable_All) {
            return type.GetMember(name, bindingFlags).FirstOrDefault();
        }
        private static MemberInfo? GetStaticMemberInfo(Type type, string name) {
            return GetMemberInfo(type, name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Get value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, string name, out object? output) {
            if (name.IsNullOrEmpty() || obj == null) {
                output = null;
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        output = GetValue(obj, mi);
                        return true;
                    } else {
                        output = null;
                        return false;
                    }
                } catch {
                    output = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Set value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, string name, object? input) {
            if (name.IsNullOrEmpty() || obj == null) {
                return false;
            } else {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try {
                    if (mi != null) {
                        SetValue(obj, input, mi);
                        return true;
                    } else {
                        return false;
                    }
                } catch {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, MemberInfo? info, out object? output) {
            output = null;
            if (obj == null || info == null) { return false; }
            try {
                if (info != null) {
                    output = GetValue(obj, info);
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Set value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, MemberInfo? info, object? input) {
            if (obj == null || info == null) { return false; }
            try {
                if (info != null) {
                    SetValue(obj, input, info);
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Get value of static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static object? GetStaticMember<T>(string name) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                return GetValue(null, member);
            }
            return default;
        }

        /// <summary>
        /// Set value of static member named <paramref name="name"/> in <typeparamref name="T"/> to <paramref name="value"/>
        /// </summary>
        public static bool SetStaticMember<T>(string name, object? value) {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null) {
                SetValue(null, value, member);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indicate whether exist a static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static bool HasStaticMember<T>(string name) {
            return GetStaticMemberInfo(typeof(T), name) != null;
        }

        /// <summary>
        /// Get the member informations with custom attribute of <paramref name="attrType"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute(this Type type, Type attrType) {
            return type.GetProperties().Where(p => p.IsDefined(attrType));
        }

        /// <summary>
        /// Get the member informations with custom attribute of <typeparamref name="T"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute<T>(this Type type) {
            return type.GetPropertiesWithCustomAttribute(typeof(T));
        }

        /// <summary>
        /// Whether the input type is struct
        /// </summary>
        /// <returns>True if the type <typeparamref name="T"/> is struct, otherwise false</returns>
        public static bool IsStruct<T>() => typeof(T).IsStruct();

        /// <summary>
        /// Whether the input type is unmanaged, and excluding any managed members
        /// </summary>
        /// <returns>True if the type <typeparamref name="T"/> is unmanaged, otherwise false</returns>
        public static bool IsUnmanaged<T>() => typeof(T).IsUnmanaged();

        /// <summary>
        /// Whether the input type contain the StructureLayout attribute
        /// </summary>
        /// <returns>True if the type <typeparamref name="T"/> contain the StructureLayout attribute, otherwise false</returns>
        public static bool HasStructureLayout<T>() => typeof(T).HasStructureLayout();

        /// <summary>
        /// Try construct an instance of <typeparamref name="T"/> with arguments <paramref name="args"/>
        /// </summary>
        /// <returns>True if succeed, otherwise false</returns>
        public static bool ConstructBy<T>([NotNullWhen(true)] out T? output, params object[] args) {
            try {
                Type[] types = new Type[args.Length];

                for (int i = 0; i < args.Length; i++) {
#pragma warning disable CS8602
                    types[i] = args[i].GetType();
#pragma warning restore CS8602
                }

                var constructor = typeof(T).GetConstructor(types);

                output = (T?)constructor?.Invoke(args) ?? default;

                return output != null;

            } catch { output = default; }

            return false;
        }

        /// <summary>
        /// Try construct an instance of <typeparamref name="T"/> with arguments <paramref name="args"/>
        /// </summary>
        /// <returns>New instance of <typeparamref name="T"/> or null if failure on constructing</returns>
        public static T? ConstructBy<T>(params object[] args) {
            ConstructBy(out T? result, args);
            return result;
        }

        /// <summary>
        /// Get the dictionary with Key as Membername and Value as PropertyInfo
        /// </summary>
        /// <returns>A dictionary with Key as Membername and Value as PropertyInfo of current type</returns>
        public static IDictionary<string, PropertyInfo> GetNamePropertyDictionary(this Type type, Func<string, string>? callbackConvertStr = null) {
            PropertyInfo[] propertyInfos = type.GetProperties();
            Dictionary<string, PropertyInfo> dict = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo propertyInfo in propertyInfos) {
                if (callbackConvertStr != null) {
                    dict[callbackConvertStr(propertyInfo.Name)] = propertyInfo;
                } else {
                    dict[propertyInfo.Name] = propertyInfo;
                }
            }
            return dict;
        }

        /// <summary>
        /// Get the dictionary with Key as Membername and Value as PropertyInfo
        /// </summary>
        /// <returns>A dictionary with Key as Membername and Value as PropertyInfo <typeparamref name="T"/> </returns>
        public static IDictionary<string, PropertyInfo> GetNamePropertyDictionary<T>(Func<string, string>? callbackConvertStr = null) {
            return typeof(T).GetNamePropertyDictionary(callbackConvertStr);
        }

        /// <summary>
        /// Get the dictionary with Key come form custom generator and Value as PropertyInfo
        /// </summary>
        /// <typeparam name="T">Type to get the dictionary</typeparam>
        /// <typeparam name="TKey">Type to be the key</typeparam>
        /// <param name="callbackGetKey">The method to get the custom key from attribute <typeparamref name="T"/></param>
        /// <returns>A dictionary with Key as value get form Attribute and Value as PropertyInfo</returns>
        public static IDictionary<TKey, PropertyInfo> GetPropertiesWithCustomAttribute<T, TKey>(this Type type, Func<T, TKey> callbackGetKey) where T : Attribute where TKey : notnull {
            PropertyInfo[] propertyInfos = type.GetProperties();
            Dictionary<TKey, PropertyInfo> dict = new Dictionary<TKey, PropertyInfo>();
            foreach (PropertyInfo propertyInfo in propertyInfos) {
                T? attr = propertyInfo.GetCustomAttribute<T>();
                if (attr != null) {
                    dict[callbackGetKey(attr)] = propertyInfo;
                }
            }
            return dict;
        }

        /// <summary>
        /// Try convert current object to <paramref name="targetType"/>
        /// </summary>
        /// <param name="targetType">Target to convert to</param>
        /// <returns>Object of target type if <paramref name="targetType"/> is not null and converted without error, otherwise null</returns>
        public static object? TryConvertTo(this object? obj, Type? targetType = null) {
            if (obj == null) { return null; }
            if (targetType == null) { return obj; }
            if(targetType.IsAssignableFrom(obj.GetType())) { return obj; }
            try {
                return Convert.ChangeType(obj, targetType.GetNotNullType());
            } catch (Exception ex) {
                ex.Print();
                return null;
            }
        }

        /// <summary>
        /// Try convert current object to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>Object of target type if converted without error, otherwise null</returns>
        public static object? TryConvertTo<T>(this object? obj) {
            return obj.TryConvertTo(typeof(T));
        }
    }
}
