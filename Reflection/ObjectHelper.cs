using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cyh.Net.Reflection
{
    public static partial class ObjectHelper
    {
        class ParameterUpdateVisitor : ExpressionVisitor
        {
            private ParameterExpression _oldParameter;
            private ParameterExpression _newParameter;

            public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                this._oldParameter = oldParameter;
                this._newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (object.ReferenceEquals(node, this._oldParameter))
                    return this._newParameter;

                return base.VisitParameter(node);
            }
        }
        abstract class MappingDelegate
        {
            public abstract Type SourceType { get; }
            public abstract Type ResultType { get; }
            public abstract object GetForward { get; }
            public abstract object GetBackward { get; }
            public abstract object SetForward { get; }
            public abstract object SetBackward { get; }
            public abstract object SelectForward { get; }
            public abstract object SelectBackward { get; }
        }
        class MappingDelegate<TSource, TResult> : MappingDelegate
        {
            public override Type SourceType => typeof(TSource);
            public override Type ResultType => typeof(TResult);
            public override object GetForward => this.GetForwardImpl;
            public override object GetBackward => this.GetBackwardImpl;
            public override object SetForward => this.SetForwardImpl;
            public override object SetBackward => this.SetBackwardImpl;
            public override object SelectForward => this.SelectForwardImpl;
            public override object SelectBackward => this.SelectBackwardImpl;

            public required Func<TSource, TResult> GetForwardImpl { get; set; }
            public required Func<TResult, TSource> GetBackwardImpl { get; set; }
            public required Action<TSource, TResult> SetForwardImpl { get; set; }
            public required Action<TResult, TSource> SetBackwardImpl { get; set; }
            public required Expression<Func<TSource, TResult>> SelectForwardImpl { get; set; }
            public required Expression<Func<TResult, TSource>> SelectBackwardImpl { get; set; }
        }
        class MappedProperty
        {
            public bool SourceReadOnly { get; set; }
            public bool TargetReadOnly { get; set; }
            public required PropertyInfo SourceProperty { get; set; }
            public required PropertyInfo TargetProperty { get; set; }
        }

        static readonly MethodInfo EnumerableEmpty;
        static readonly MethodInfo ArrayEmpty;
        static readonly Dictionary<string, MappingDelegate> MappingDelegates;
        static readonly Dictionary<PropertyInfo, Action<object?, object?>> SetPropertyActions;
        static readonly Dictionary<PropertyInfo, Func<object?, object?>> GetPropertyFuncs;
        static readonly Dictionary<Type, Delegate> ParameterlessContructDelegates;
        static readonly MethodInfo ExpressionLambda__Expr_Bool_ParamExprEnumerable__;
        static readonly MethodInfo Mapper__GetSetValueAction_;

        static readonly Type[] NumericTypes = {
            typeof(byte),typeof(sbyte),typeof(ushort),typeof(short),
            typeof(int),typeof(uint),typeof(long),typeof(ulong),
            typeof(float),typeof(double),typeof(decimal)
            };
        static ObjectHelper()
        {
            EnumerableEmpty = typeof(Enumerable).GetMethod("Empty", BindingFlags.Static | BindingFlags.Public)!;
            ArrayEmpty = typeof(Array).GetMethod("Empty", BindingFlags.Static | BindingFlags.Public)!;
            MappingDelegates = new();
            SetPropertyActions = new();
            GetPropertyFuncs = new();
            ParameterlessContructDelegates = new();
            {
                MethodInfo? methodForSelectorExpr = typeof(Expression).GetMethod("Lambda", 1, [typeof(Expression), typeof(bool), typeof(IEnumerable<ParameterExpression>)]);
                Debug.Assert(methodForSelectorExpr != null);
                ExpressionLambda__Expr_Bool_ParamExprEnumerable__ = methodForSelectorExpr;
            }
            Mapper__GetSetValueAction_ = typeof(ObjectHelper).GetMethod("Impl_GetSetValueAction", BindingFlags.Static | BindingFlags.NonPublic)!;
        }
        static Action<TSource?, TResult?> Impl_GetSetValueAction<TSource, TResult>(Action<object?, object?> func)
        {
            return (src, dst) => func(src, dst);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetTypeHash(Type type, params Type[] types)
        {
            string[] names = types.Select(t => t.Name).ToArray();
            string totalStr = type.Name + string.Join("", names);
            byte[] bSrc = SHA512.HashData(totalStr.GetBytesUtf8());
            return bSrc.GetStringAsUtf8();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void GetSetDefaultMemberAssignment(ref List<MemberAssignment> assignments, List<PropertyInfo> setDefaultProperties)
        {
            for (int i = 0; i < setDefaultProperties.Count; ++i)
            {
                PropertyInfo setDefaultProperty = setDefaultProperties[i];
                Type unusedPropertyType = setDefaultProperty.PropertyType;
                Expression defaultExpression;
                if (unusedPropertyType.IsValueType)
                {
                    ConstructBy(unusedPropertyType, out object? inst);
                    defaultExpression = Expression.Constant(inst, unusedPropertyType);
                }
                else
                {
                    defaultExpression = Expression.Constant(null, unusedPropertyType);
                }
                assignments.Add(Expression.Bind(setDefaultProperty, defaultExpression));
            }
        }
        static Func<object?, object?> CreateGetValueDelegate(PropertyInfo propertyInfo)
        {
            Console.WriteLine($"Create GetValueDelegate: {propertyInfo.DeclaringType}.{propertyInfo.Name}");
            ParameterExpression p = Expression.Parameter(typeof(object), "p");
            UnaryExpression cast;
            Type? declaringType = propertyInfo.DeclaringType;
            if (declaringType == null)
            {
                throw new InvalidOperationException("Property does not have a declaring type.");
            }
            if (declaringType.IsValueType)
            {
                cast = Expression.Convert(p, declaringType);
            }
            else
            {
                cast = Expression.TypeAs(p, declaringType);
            }
            MethodInfo? getValueMethod = propertyInfo.GetGetMethod(true);
            if (getValueMethod == null)
            {
                throw new InvalidOperationException("Property does not have a getter.");
            }
            Expression<Func<object?, object?>> expr = Expression.Lambda<Func<object?, object?>>(Expression.TypeAs(Expression.Call(cast, getValueMethod), typeof(object)), p);
            return expr.Compile();
        }
        static Action<object?, object?> CreateSetValueDelegate(PropertyInfo propertyInfo)
        {
            Console.WriteLine($"Create SetValueDelegate: {propertyInfo.DeclaringType}.{propertyInfo.Name}");
            ParameterExpression p = Expression.Parameter(typeof(object), "p");
            ParameterExpression v = Expression.Parameter(typeof(object), "v");
            UnaryExpression cast;
            Type? declaringType = propertyInfo.DeclaringType;
            if (declaringType == null)
            {
                throw new InvalidOperationException("Property does not have a declaring type.");
            }
            if (declaringType.IsValueType)
            {
                cast = Expression.Convert(p, declaringType);
            }
            else
            {
                cast = Expression.TypeAs(p, declaringType);
            }
            UnaryExpression castValue;
            if (propertyInfo.PropertyType.IsValueType)
            {
                castValue = Expression.Convert(v, propertyInfo.PropertyType);
            }
            else
            {
                castValue = Expression.TypeAs(v, propertyInfo.PropertyType);
            }
            MethodInfo? setValueMethod = propertyInfo.GetSetMethod(true);
            if (setValueMethod == null)
            {
                throw new InvalidOperationException("Property does not have a setter.");
            }
            Expression<Action<object?, object?>> expr = Expression.Lambda<Action<object?, object?>>(Expression.Call(cast, setValueMethod, castValue), p, v);
            return expr.Compile();
        }
        static MappingDelegate CreateMappingDelegates(Type sourceType, Type targetType, string mark)
        {
            Type retValType = typeof(MappingDelegate<,>).MakeGenericType(sourceType, targetType);
            ConstructBy(retValType, out object? retVal);
            Debug.Assert(retValType != null);

            PropertyInfo[] targetProps = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] sourceProps = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            List<PropertyInfo> unusedSourceProperties = sourceProps.ToList();
            List<PropertyInfo> unusedTargetProperties = new();
            List<MappedProperty> mappedPropertyPairs = new();
            for (int i = 0; i < targetProps.Length; i++)
            {
                PropertyInfo targetProperty = targetProps[i];
                MapFromAttribute? mapFromAttribute = targetProperty.GetCustomAttributes<MapFromAttribute>(true).FirstOrDefault(x => x.SourceType == sourceType);

                bool bypass;
                {
                    if (mapFromAttribute == null)
                    {
                        bypass = true;
                    }
                    else
                    {
                        bypass = false;
                        IEnumerable<string> marks;
                        {
                            if (mapFromAttribute.Mark.IsNullOrEmpty())
                            {
                                marks = [];
                            }
                            else
                            {
                                marks = mapFromAttribute.Mark.Split(',').Select(x => x.Trim());
                            }
                        }
                        if (marks.Any() && !marks.Contains(mark))
                        {
                            bypass = true;
                        }
                    }
                }

                if (!bypass)
                {
                    Debug.Assert(mapFromAttribute != null);
                    PropertyInfo? sourceProperty = sourceProps.FirstOrDefault(x => x.Name == mapFromAttribute.SourcePropertyName);
                    if (sourceProperty == null) continue;
                    unusedSourceProperties.Remove(sourceProperty);
                    mappedPropertyPairs.Add(new MappedProperty
                    {
                        SourceProperty = sourceProperty,
                        TargetProperty = targetProperty,
                        SourceReadOnly = mapFromAttribute.IsSourceReadOnly,
                        TargetReadOnly = mapFromAttribute.IsTargetReadOnly
                    });
                }
                else
                {
                    unusedTargetProperties.Add(targetProperty);
                }
            }

            object selectForward;
            object getForward;
            object setForward;
            {
                ParameterExpression parameter = Expression.Parameter(sourceType, "x");
                List<MemberAssignment> assignments = new();
                List<PropertyInfo> setDefaultProperties = new();
                setDefaultProperties.AddRange(unusedTargetProperties);
                for (int i = 0; i < mappedPropertyPairs.Count; i++)
                {
                    MappedProperty mappedPropertyPair = mappedPropertyPairs[i];
                    if (mappedPropertyPair.TargetReadOnly)
                    {
                        setDefaultProperties.Add(mappedPropertyPair.TargetProperty);
                        continue;
                    }
                    MemberExpression sourceProperty = Expression.Property(parameter, mappedPropertyPair.SourceProperty);
                    assignments.Add(Expression.Bind(mappedPropertyPair.TargetProperty, sourceProperty));
                }
                GetSetDefaultMemberAssignment(ref assignments, setDefaultProperties);
                NewExpression newExpression = Expression.New(targetType);
                MemberInitExpression memberInitExpression = Expression.MemberInit(newExpression, assignments);
                Type funcType = typeof(Func<,>).MakeGenericType(sourceType, targetType);
                Type actType = typeof(Action<,>).MakeGenericType(sourceType, targetType);
                selectForward = ExpressionLambda__Expr_Bool_ParamExprEnumerable__
                    .MakeGenericMethod(funcType).Invoke(null, [memberInitExpression, false, new ParameterExpression[] { parameter }])!;
                MethodInfo compile = selectForward.GetType().GetMethod("Compile", BindingFlags.Public | BindingFlags.Instance, [])!;
                Debug.Assert(compile != null);
                getForward = compile.Invoke(selectForward, null)!;
                Action<object?, object?> action = (src, dst) =>
                {
                    for (int i = 0; i < mappedPropertyPairs.Count; i++)
                    {
                        MappedProperty mappedPropertyPair = mappedPropertyPairs[i];
                        if (mappedPropertyPair.TargetReadOnly) continue;
                        mappedPropertyPair.TargetProperty.SetValueEx(dst, mappedPropertyPair.SourceProperty.GetValueEx(src));
                    }
                };
                setForward = Mapper__GetSetValueAction_.MakeGenericMethod(sourceType, targetType).Invoke(null, [action])!;
            }

            object selectBackward;
            object getBackward;
            object setBackward;
            {
                ParameterExpression parameter = Expression.Parameter(targetType, "x");
                List<MemberAssignment> assignments = new();
                List<PropertyInfo> setDefaultProperties = new();
                setDefaultProperties.AddRange(unusedSourceProperties);
                for (int i = 0; i < mappedPropertyPairs.Count; i++)
                {
                    MappedProperty mappedPropertyPair = mappedPropertyPairs[i];
                    if (mappedPropertyPair.SourceReadOnly)
                    {
                        setDefaultProperties.Add(mappedPropertyPair.SourceProperty);
                        continue;
                    }
                    MemberExpression targetProperty = Expression.Property(parameter, mappedPropertyPair.TargetProperty);
                    assignments.Add(Expression.Bind(mappedPropertyPair.SourceProperty, targetProperty));
                }
                GetSetDefaultMemberAssignment(ref assignments, setDefaultProperties);
                NewExpression newExpression = Expression.New(sourceType);
                MemberInitExpression memberInitExpression = Expression.MemberInit(newExpression, assignments);
                Type funcType = typeof(Func<,>).MakeGenericType(targetType, sourceType);
                Type actType = typeof(Action<,>).MakeGenericType(targetType, sourceType);
                selectBackward = ExpressionLambda__Expr_Bool_ParamExprEnumerable__
                    .MakeGenericMethod(funcType).Invoke(null, [memberInitExpression, false, new ParameterExpression[] { parameter }])!;
                MethodInfo compile = selectBackward.GetType().GetMethod("Compile", BindingFlags.Public | BindingFlags.Instance, [])!;
                Debug.Assert(compile != null);
                getBackward = compile.Invoke(selectBackward, null)!;

                Action<object?, object?> action = (dst, src) =>
                {
                    for (int i = 0; i < mappedPropertyPairs.Count; i++)
                    {
                        MappedProperty mappedPropertyPair = mappedPropertyPairs[i];
                        if (mappedPropertyPair.SourceReadOnly) continue;
                        mappedPropertyPair.SourceProperty.SetValueEx(src, mappedPropertyPair.TargetProperty.GetValueEx(dst));
                    }
                };
                setBackward = Mapper__GetSetValueAction_.MakeGenericMethod(targetType, sourceType).Invoke(null, [action])!;
            }
            MappingDelegate? _retVal = (MappingDelegate?)retVal;

            Debug.Assert(_retVal != null);

            Debug.Assert(selectForward != null);
            Debug.Assert(getForward != null);
            Debug.Assert(setForward != null);

            Debug.Assert(selectBackward != null);
            Debug.Assert(getBackward != null);
            Debug.Assert(setBackward != null);

            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.SelectForwardImpl), selectForward));
            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.GetForwardImpl), getForward));
            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.SetForwardImpl), setForward));
            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.SelectBackwardImpl), selectBackward));
            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.GetBackwardImpl), getBackward));
            Debug.Assert(TrySetMember(retVal, nameof(MappingDelegate<int, int>.SetBackwardImpl), setBackward));

            return _retVal;
        }
        static Func<object?, object?> GetGetValueDelegate(PropertyInfo propertyInfo)
        {
            if (!CachePropertyDelegates)
            {
                return CreateGetValueDelegate(propertyInfo);
            }
            if (!GetPropertyFuncs.TryGetValue(propertyInfo, out Func<object?, object?>? func))
            {
                func = CreateGetValueDelegate(propertyInfo);
                GetPropertyFuncs[propertyInfo] = func;
            }
            return func;
        }
        static Action<object?, object?> GetSetValueDelegate(PropertyInfo propertyInfo)
        {
            if (!CachePropertyDelegates)
            {
                return CreateSetValueDelegate(propertyInfo);
            }
            if (!SetPropertyActions.TryGetValue(propertyInfo, out Action<object?, object?>? func))
            {
                func = CreateSetValueDelegate(propertyInfo);
                SetPropertyActions[propertyInfo] = func;
            }
            return func;
        }
        static MappingDelegate GetMappingDelegates(Type sourceType, Type targetType, string mark)
        {
            string hash;
            {
                if (mark.IsNullOrEmpty())
                {
                    hash = GetTypeHash(sourceType, targetType);
                }
                else
                {
                    hash = $"{GetTypeHash(sourceType, targetType)}__{mark}";
                }
            }
            if (!MappingDelegates.TryGetValue(hash, out MappingDelegate? mappingDelegate))
            {
                mappingDelegate = CreateMappingDelegates(sourceType, targetType, mark);
                MappingDelegates[hash] = mappingDelegate;
            }
            return mappingDelegate;
        }
        static MappingDelegate<TSource, TResult> GetMappingDelegates<TSource, TResult>(string mark)
        {
            return (MappingDelegate<TSource, TResult>)GetMappingDelegates(typeof(TSource), typeof(TResult), mark);
        }
        static object? GetValue(object? instance, MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(instance);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue(instance);
            }
            else
            {
                return null;
            }
        }
        static void SetValue(object? instance, object? value, MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(instance, value);
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(instance, value);
            }
            else
            {
                throw new InvalidOperationException("nuknow member information");
            }
        }
        static MemberInfo[] GetMemberInfos(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return type.GetMembers(bindingFlags);
        }
        static MemberInfo? GetMemberInfo(Type type, string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return type.GetMember(name, bindingFlags).FirstOrDefault();
        }
        static MemberInfo? GetStaticMemberInfo(Type type, string name)
        {
            return GetMemberInfo(type, name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
    public static partial class ObjectHelper
    {
        public static bool CachePropertyDelegates = true;
        public static bool CacheSelectExpressions = true;

        /// <summary>
        /// Get value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, string name, out object? output)
        {
            if (name.IsNullOrEmpty() || obj == null)
            {
                output = null;
                return false;
            }
            else
            {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try
                {
                    if (mi != null)
                    {
                        output = GetValue(obj, mi);
                        return true;
                    }
                    else
                    {
                        output = null;
                        return false;
                    }
                }
                catch
                {
                    output = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Set value of member named <paramref name="name"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, string name, object? input)
        {
            if (name.IsNullOrEmpty() || obj == null)
            {
                return false;
            }
            else
            {
                MemberInfo? mi = GetMemberInfo(obj.GetType(), name);
                try
                {
                    if (mi != null)
                    {
                        SetValue(obj, input, mi);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TryGetMember(object? obj, MemberInfo? info, out object? output)
        {
            output = null;
            if (obj == null || info == null) { return false; }
            try
            {
                if (info != null)
                {
                    output = GetValue(obj, info);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Set value of member match the <paramref name="info"/> in <paramref name="obj"/>
        /// </summary>
        public static bool TrySetMember(object? obj, MemberInfo? info, object? input)
        {
            if (obj == null || info == null) { return false; }
            try
            {
                if (info != null)
                {
                    SetValue(obj, input, info);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get value of static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static object? GetStaticMember<T>(string name)
        {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null)
            {
                return GetValue(null, member);
            }
            return default;
        }

        /// <summary>
        /// Set value of static member named <paramref name="name"/> in <typeparamref name="T"/> to <paramref name="value"/>
        /// </summary>
        public static bool SetStaticMember<T>(string name, object? value)
        {
            MemberInfo? member = GetStaticMemberInfo(typeof(T), name);
            if (member != null)
            {
                SetValue(null, value, member);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indicate whether exist a static member named <paramref name="name"/> in <typeparamref name="T"/>
        /// </summary>
        public static bool HasStaticMember<T>(string name)
        {
            return GetStaticMemberInfo(typeof(T), name) != null;
        }

        /// <summary>
        /// Get the member informations with custom attribute of <paramref name="attrType"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute(this Type type, Type attrType)
        {
            return type.GetProperties().Where(p => p.IsDefined(attrType));
        }

        /// <summary>
        /// Get the member informations with custom attribute of <typeparamref name="T"/>
        /// </summary>
        public static IEnumerable<PropertyInfo> GetPropertiesWithCustomAttribute<T>(this Type type)
        {
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
        /// Indicate whether a type is collection
        /// </summary>
        /// <returns>True if current type is collection, otherwise false</returns>
        public static bool IsCollection(this Type type)
        {
            return type.IsGenericType || type.IsArray;
        }

        /// <summary>
        /// Indicate whether a type is collection
        /// </summary>
        /// <returns>True if the type is collection, otherwise false</returns>
        public static bool IsCollection<T>()
        {
            return typeof(T).IsCollection();
        }

        /// <summary>
        /// Whether the input type contain the StructureLayout attribute
        /// </summary>
        /// <returns>True if the type <typeparamref name="T"/> contain the StructureLayout attribute, otherwise false</returns>
        public static bool HasStructureLayout<T>() => typeof(T).HasStructureLayout();

        /// <summary>
        /// Try construct an instance of <paramref name="type"/> with arguments <paramref name="args"/>
        /// </summary>
        /// <returns>True if succeed, otherwise false</returns>
        public static bool ConstructBy(Type type, [NotNullWhen(true)] out object? output, params object[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    if (!ParameterlessContructDelegates.TryGetValue(type, out Delegate? @delegate))
                    {
                        LambdaExpression expression = Expression.Lambda(Expression.New(type));
                        @delegate = expression.Compile();
                        ParameterlessContructDelegates[type] = @delegate;
                    }
                    output = @delegate.DynamicInvoke();
                    return output != null;
                }

                Type[] types = new Type[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
#pragma warning disable CS8602
                    types[i] = args[i].GetType();
#pragma warning restore CS8602
                }

                ConstructorInfo? constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic, types);

                output = constructor?.Invoke(args) ?? default;

                return output != null;

            }
            catch { output = default; }

            return false;
        }

        /// <summary>
        /// Try construct an instance of <typeparamref name="T"/> with arguments <paramref name="args"/>
        /// </summary>
        /// <returns>True if succeed, otherwise false</returns>
        public static bool ConstructBy<T>([NotNullWhen(true)] out T? output, params object[] args)
        {
            if (ConstructBy(typeof(T), out object? meta, args))
            {
                output = (T)meta;
                return true;
            }
            else
            {
                output = default;
                return false;
            }
        }

        /// <summary>
        /// Try construct an instance of <typeparamref name="T"/> with arguments <paramref name="args"/>
        /// </summary>
        /// <returns>New instance of <typeparamref name="T"/> or null if failure on constructing</returns>
        public static T? ConstructBy<T>(params object[] args)
        {
            ConstructBy(out T? result, args);
            return result;
        }

        /// <summary>
        /// Get the dictionary with Key as Membername and Value as PropertyInfo
        /// </summary>
        /// <returns>A dictionary with Key as Membername and Value as PropertyInfo of current type</returns>
        public static IDictionary<string, PropertyInfo> GetNamePropertyDictionary(this Type type, Func<string, string>? callbackConvertStr = null)
        {
            PropertyInfo[] propertyInfos = type.GetProperties();
            Dictionary<string, PropertyInfo> dict = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (callbackConvertStr != null)
                {
                    dict[callbackConvertStr(propertyInfo.Name)] = propertyInfo;
                }
                else
                {
                    dict[propertyInfo.Name] = propertyInfo;
                }
            }
            return dict;
        }

        /// <summary>
        /// Get the dictionary with Key as Membername and Value as PropertyInfo
        /// </summary>
        /// <returns>A dictionary with Key as Membername and Value as PropertyInfo <typeparamref name="T"/> </returns>
        public static IDictionary<string, PropertyInfo> GetNamePropertyDictionary<T>(Func<string, string>? callbackConvertStr = null)
        {
            return typeof(T).GetNamePropertyDictionary(callbackConvertStr);
        }

        /// <summary>
        /// Get the dictionary with Key come form custom generator and Value as PropertyInfo
        /// </summary>
        /// <typeparam name="T">Type to get the dictionary</typeparam>
        /// <typeparam name="TKey">Type to be the key</typeparam>
        /// <param name="callbackGetKey">The method to get the custom key from attribute <typeparamref name="T"/></param>
        /// <returns>A dictionary with Key as value get form Attribute and Value as PropertyInfo</returns>
        public static IDictionary<TKey, PropertyInfo> GetPropertiesWithCustomAttribute<T, TKey>(this Type type, Func<T, TKey> callbackGetKey) where T : Attribute where TKey : notnull
        {
            PropertyInfo[] propertyInfos = type.GetProperties();
            Dictionary<TKey, PropertyInfo> dict = new Dictionary<TKey, PropertyInfo>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                T? attr = propertyInfo.GetCustomAttribute<T>();
                if (attr != null)
                {
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
        public static object? TryConvertTo(this object? obj, Type? targetType = null)
        {
            if (obj == null) { return null; }
            if (targetType == null) { return obj; }
            if (targetType.IsAssignableFrom(obj.GetType())) { return obj; }
            try
            {
                return Convert.ChangeType(obj, targetType.RemoveNullable());
            }
            catch (Exception ex)
            {
                ex.Print();
                return null;
            }
        }

        /// <summary>
        /// Try convert current object to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>Object of target type if converted without error, otherwise null</returns>
        public static object? TryConvertTo<T>(this object? obj)
        {
            return obj.TryConvertTo(typeof(T));
        }

        /// <summary>
        /// Call instance method by name
        /// </summary>
        /// <param name="obj">instance</param>
        /// <param name="methodName">Name of method to invoke</param>
        /// <param name="parameters">Parameters of called method</param>
        /// <returns>Return value when called method has return value, otherwise null</returns>
        public static object? CallMethod(this object obj, string methodName, params object[]? parameters)
        {
            try
            {
                Type[] types = parameters?.Select(p => p.GetType()).ToArray() ?? Array.Empty<Type>();
                MethodInfo? method = obj.GetType().GetMethod(methodName, types);
                if (method != null)
                {
                    return method.Invoke(obj, parameters);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Call static method by name
        /// </summary>
        /// <typeparam name="T">Type contain the target method</typeparam>
        /// <param name="methodName">Name of method to invoke</param>
        /// <param name="parameters">Parameters of called method<</param>
        /// <returns>Return value when called method has return value, otherwise null</returns>
        public static object? CallStaticMethod<T>(string methodName, params object[]? parameters)
        {
            try
            {
                Type[] types = parameters?.Select(p => p.GetType()).ToArray() ?? Array.Empty<Type>();
                MethodInfo? method = typeof(T).GetMethod(methodName, types);
                if (method != null)
                {
                    return method.Invoke(null, parameters);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set property value by Expression instead of PropertyInfo.SetValue()
        /// </summary>
        /// <param name="propertyInfo">Target property</param>
        /// <param name="target">Target object instance</param>
        /// <param name="value">Value to set</param>
        public static void SetValueEx(this PropertyInfo propertyInfo, object? target, object? value)
        {
            GetSetValueDelegate(propertyInfo).Invoke(target, value);
        }

        /// <summary>
        /// Get property value by Expression instead of PropertyInfo.GetValue()
        /// </summary>
        /// <param name="propertyInfo">Target property</param>
        /// <param name="target">Target object instance</param>
        /// <returns>Value of property</returns>
        public static object? GetValueEx(this PropertyInfo propertyInfo, object? target)
        {
            return GetGetValueDelegate(propertyInfo).Invoke(target);
        }

        /// <summary>
        /// Get Expression accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns>x =&gt; new <typeparamref name="TResult"/>{ ... }</returns>
        public static Expression<Func<TSource, TResult>> GetMapForwardExpression<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).SelectForwardImpl;
        }

        /// <summary>
        /// Get Expression accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns>x =&gt; new <typeparamref name="TSource"/>{ ... }</returns>
        public static Expression<Func<TResult, TSource>> GetMapBackwardExpression<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).SelectBackwardImpl;
        }

        /// <summary>
        /// Get convert function from <typeparamref name="TSource"/> to <typeparamref name="TResult"/> accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns><typeparamref name="TResult"/> Func(<typeparamref name="TSource"/> src) </returns>
        public static Func<TSource, TResult> GetMapCastForwardDelegate<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).GetForwardImpl;
        }

        /// <summary>
        /// Get convert function from <typeparamref name="TResult"/> to <typeparamref name="TSource"/> accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns><typeparamref name="TSource"/> Func(<typeparamref name="TResult"/> src) </returns>
        public static Func<TResult, TSource> GetMapCastBackwardDelegate<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).GetBackwardImpl;
        }

        /// <summary>
        /// Get set member function from <typeparamref name="TSource"/> to <typeparamref name="TResult"/> accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns>void Func(<typeparamref name="TSource"/> src, <typeparamref name="TResult"/> dst)</returns>
        public static Action<TSource, TResult> GetMapAssignForwardDelegate<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).SetForwardImpl;
        }

        /// <summary>
        /// Get set member function from <typeparamref name="TResult"/> to <typeparamref name="TSource"/> accroding to <see cref="MapFromAttribute"/>
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TResult">Target type</typeparam>
        /// <param name="mark">Aadditional mark for identity</param>
        /// <returns>void Func(<typeparamref name="TResult"/> src, <typeparamref name="TSource"/> dst)</returns>
        public static Action<TResult, TSource> GetMapAssignBackwardDelegate<TSource, TResult>(string mark = "")
        {
            return GetMappingDelegates<TSource, TResult>(mark).SetBackwardImpl;
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
        /// Whether the input type is number
        /// </summary>
        /// <returns>True if the input type is number, otherwise false</returns>
        public static bool IsNumeric(this Type type)
        {
            if (type == null) { return false; }
            return type.IsAnyOf(NumericTypes);
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
                FieldInfo[] members = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo member in members)
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
            if (type == null) { return false; }
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            return false;
        }

        /// <summary>
        /// Indicate whether a type is dotnet builtin type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>The type is dotnet builtin type</returns>
        public static bool IsBuiltinType(this Type type)
        {
            if (type == null) return false;
            return type.Assembly.GetName().Name == "System.Private.CoreLib";
        }

        /// <summary>
        /// Get not null type
        /// </summary>
        /// <returns>Type of Nullable`T`.Value or current type if not Nullable`T`</returns>
        public static Type RemoveNullable(this Type type)
        {
            if (type.IsNullableWrapped())
            {
#pragma warning disable CS8602
                return type.GetGenericArguments().First();
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
        /// Indicate whether a type is collection type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if the type is collection type, otherwise false</returns>
        public static bool IsCollectionType(this Type type)
        {
            return type.IsGenericType || type.IsArray;
        }

        /// <summary>
        /// Indicate whether a type is collection type
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        /// <returns>True if the type is collection type, otherwise false</returns>
        public static bool IsCollectionType<T>()
        {
            return typeof(T).IsCollectionType();
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
                ConstructorInfo? constructor = type.GetConstructor(constructorTypes);
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
        /// Update predicate expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalExpression"></param>
        /// <param name="additionalExpression"></param>
        /// <param name="_and"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> UpdateExpression<T>(this Expression<Func<T, bool>>? originalExpression, Expression<Func<T, bool>> additionalExpression, bool _and)
        {
            if (originalExpression == null)
            {
                return additionalExpression;
            }
            // get the visitor
            ParameterUpdateVisitor visitor = new ParameterUpdateVisitor(originalExpression.Parameters.First(), additionalExpression.Parameters.First());
            // replace the parameter in the original expression
            originalExpression = visitor.Visit(originalExpression) as Expression<Func<T, bool>>;
#pragma warning disable CS8602
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
#pragma warning restore CS8602
            // and return a new lambda, that will do what you want.
            // NOTE that the binExp has reference only to te newExp.Parameters[0] (there is only 1) parameter, and no other
            return Expression.Lambda<Func<T, bool>>(binExp, originalExpression.Parameters);
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
        /// Ensure returning type IEnumerable&lt;Any&gt; or Any[] would be Enumerable.Empty&lt;Any&gt;() or Any[0] instead of null
        /// </summary>
        /// <param name="type">IEnumerable&lt;Any&gt; or Any[]</param>
        /// <param name="collectionObject">Enumerable.Empty&lt;Any&gt;() or Any[]</param>
        /// <returns>Enumerable.Empty&lt;Any&gt;() or Any[] is created, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateEmptyCollection(this Type type, [NotNullWhen(true)] out object? collectionObject)
        {
            if (type.IsGenericType)
            {
                Type def = type.GetGenericTypeDefinition();
                if (def == typeof(IEnumerable<>))
                {
                    Type[] args = type.GetGenericArguments();
                    try
                    {
                        MethodInfo method = EnumerableEmpty.MakeGenericMethod(args);
                        collectionObject = method.Invoke(null, null)!;
                        return true;
                    }
                    catch
                    {
                        collectionObject = default;
                        return false;
                    }
                }
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType()!;
                try
                {
                    MethodInfo method = ArrayEmpty.MakeGenericMethod(elementType);
                    collectionObject = method.Invoke(null, null)!;
                    return true;
                }
                catch
                {
                    collectionObject = default;
                    return false;
                }
            }
            collectionObject = default;
            return false;
        }

        /// <summary>
        /// Ensure returning type IEnumerable&lt;Any&gt; or Any[] would be Enumerable.Empty&lt;Any&gt;() or Any[0] instead of null
        /// </summary>
        /// <typeparam name="T">IEnumerable&lt;Any&gt; or Any[]</typeparam>
        /// <param name="collectionObject">Enumerable.Empty&lt;Any&gt;() or Any[]</param>
        /// <returns>Enumerable.Empty&lt;Any&gt;() or Any[] is created, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateEmptyCollection<T>([NotNullWhen(true)] out T? collectionObject)
        {
            bool success = typeof(T).CreateEmptyCollection(out object? _enumerableEmpty);
            if (!success)
            {
                collectionObject = default;
                return false;
            }
            try
            {
                collectionObject = (T?)_enumerableEmpty;
                return collectionObject != null;
            }
            catch
            {
                collectionObject = default;
                return false;
            }
        }
    }
}
