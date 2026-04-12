using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Mono;
using Core.Singleton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.DI
{
    /// <summary>
    /// 依赖容器
    /// </summary>
    public class DIContainer
    {
        // 绑定标志
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        // 接口类型映射
        private static readonly Dictionary<Type, object> _interfaceMap = new();
        // 实例类型映射
        private static readonly Dictionary<Type, object> _instanceMap = new();
        // 存储接口类型与其默认实现类型的映射（由 BindSingleton 填充）
        private static readonly Dictionary<Type, Type> _interfaceToImpl = new();
        // 记录类型是否为单例（默认为瞬态）
        private static readonly Dictionary<Type, bool> _lifetimes = new();

        private static readonly Stack<Type> _resolveStack = new();

        /// <summary>
        /// 绑定类型单例，这个类型只能作为单例使用
        /// </summary>
        /// <typeparam name="TInstance">作为单例的类型</typeparam>
        /// <typeparam name="TInterface">类型接口</typeparam>
        public static void BindSingleton<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface
        {
            BindType<TInterface, TInstance>();
            _lifetimes.TryAdd(typeof(TInstance), true);
            
            // 原有的实例创建逻辑...
            // 注意：此处建议延迟创建实例，改为在首次使用时再创建，避免初始化顺序问题
        }

        /// <summary>
        /// 绑定类型，若注入的类型是接口，则需要在注入前绑定该接口对应的实例类型，否则无法注入接口类型的参数
        /// 若需将绑定的类型作为单例，而使用BindSingleton方法
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <typeparam name="TInstance">实例类型</typeparam>
        public static void BindType<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface
        {
            // 记录接口与实现类型的映射
            _interfaceToImpl.TryAdd(typeof(TInterface), typeof(TInstance));
        }

        /// <summary>
        /// 获取依赖，不存在则自动创建，会递归创建依赖项
        /// </summary>
        /// <param name="type">实例/接口类型</param>
        /// <returns>返回自动创建的实例，若参数为null，则返回null</returns>
        /// <exception cref="Exception">若参数为接口类型，但未在_interfaceToImpl中找到映射则抛出异常</exception>
        public static object Resolve(Type type)
        {
            if (type == null) return null;
            
            if (_resolveStack.Contains(type))
                throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", _resolveStack.Reverse())} -> {type}");
            
            _resolveStack.Push(type);
            try
            {
                // 如果已经是实例（接口或具体类型），直接返回
                if (_interfaceMap.TryGetValue(type, out var existing))
                    return existing;
                if (_instanceMap.TryGetValue(type, out existing))
                    return existing;

                var implType = type;
                // 如果是接口，找到其映射的具体类型
                if (type.IsInterface)
                {
                    // 从映射中查找
                    var find = _interfaceToImpl.TryGetValue(type, out var itnType);
                    implType = find ? itnType : throw new Exception($"No implementation registered for interface {type.Name}");
                    // 是接口且是Mono
                    if (typeof(Component).IsAssignableFrom(implType))
                    {
                        return CreateMono(type, implType, _lifetimes.GetValueOrDefault(implType));
                    }
                }
                else
                {
                    // 不是接口且是Mono
                    if (typeof(Component).IsAssignableFrom(implType))
                    {
                        return CreateMono(null, implType, _lifetimes.GetValueOrDefault(type));
                    }
                }
            
                // 不是接口和Mono，解析具体类型，创建类型实例
                return Create(type, implType, _lifetimes.GetValueOrDefault(implType));
            }
            finally
            {
                _resolveStack.Pop();
            }
        }
        
        /// <summary>
        /// 通过反射创建实例。先尝试构造函数注入（选择参数都能解析的构造函数），再对标记了[Inject]的字段/属性进行补充注入
        /// </summary>
        /// <param name="isSingleton">是否是单例，true创建为单例，false则是瞬态对象</param>
        /// <param name="constructorArgs">构造参数</param>
        /// <typeparam name="T">非接口引用类型</typeparam>
        /// <returns>新类型实例</returns>
        /// <exception cref="ArgumentException">重复创建单例类型则抛出异常</exception>
        public static T Create<T>(bool isSingleton = false, params ParameterArg[] constructorArgs) where T : class
        {
            var instance = _instanceMap.GetValueOrDefault(typeof(T)) ?? _interfaceMap.GetValueOrDefault(typeof(T));
            if (instance != null)
                return instance as T;
            
            // 先通过构造函数创建实例
            var newInstance = CreateInstanceWithConstructorInjection(typeof(T), constructorArgs);
            // 注入字段/属性
            InjectIntoInstance(newInstance);
            
            Debug.Log($"创建类型：{typeof(T)}");
            
            // 不是单例直接返回
            if (!isSingleton) return (T)newInstance;
            return _instanceMap.TryAdd(typeof(T), newInstance) ? (T)newInstance : throw new ArgumentException($"{typeof(T)} already exists.");
        }

        /// <summary>
        /// 通过反射创建实例。先尝试构造函数注入（选择参数都能解析的构造函数），再对标记了[Inject]的字段/属性进行补充注入
        /// </summary>
        /// <param name="interfaceType">实例类型</param>
        /// <param name="instanceType"></param>
        /// <param name="isSingleton">是否是单例，true创建为单例，false则是瞬态对象</param>
        /// <param name="constructorArgs">构造参数</param>
        /// <returns>新类型实例</returns>
        /// <exception cref="ArgumentException">重复创建单例类型则抛出异常</exception>
        public static object Create(Type interfaceType, Type instanceType, bool isSingleton = false, params ParameterArg[] constructorArgs)
        {
            if (instanceType == null)
                return null;
            
            // 通过构造函数创建实例
            var instance = CreateInstanceWithConstructorInjection(instanceType, constructorArgs);
            // 注入字段/属性
            InjectIntoInstance(instance);
            
            Debug.Log($"创建类型：{instanceType}");
            
            // 不是单例直接返回
            if (!isSingleton) return instance;
            return _instanceMap.TryAdd(instanceType, instance) && (interfaceType == null || _interfaceMap.TryAdd(interfaceType, instance)) 
                ? instance
                : throw new ArgumentException($"{interfaceType}-{instanceType} already exists.");
        }

        /// <summary>
        /// 创建mono类型，通过new一个GameObject添加Type类型脚本来实现的
        /// </summary>
        /// <param name="interfaceType">接口类型，可以为null，为null则不存储到_interfaceMap中</param>
        /// <param name="instanceType">实例类型，不能为null</param>
        /// <param name="isSingleton">Mono是否是单例</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">重复创建单例类型抛出异常</exception>
        private static object CreateMono(Type interfaceType, Type instanceType, bool isSingleton)
        {
            var go = new GameObject(instanceType.Name);
            if (isSingleton) 
                Object.DontDestroyOnLoad(go);
            var component = go.AddComponent(instanceType);
            InjectIntoInstance(component); // 添加注入
            if (isSingleton)
            {
                _instanceMap.TryAdd(instanceType, component);
                if (interfaceType != null) _interfaceMap.TryAdd(interfaceType, component);
            }
            return component;
        }

        /// <summary>
        /// 传入GameObject对象为其挂载泛型类型脚本，并初始化其中被Inject修饰的字段/属性
        /// </summary>
        /// <param name="obj">GameObject对象</param>
        /// <typeparam name="T">可挂载的组件类型</typeparam>
        /// <returns>若参数为null，则返回null；否则返回T类型；若类型不能添加呢？？？</returns>
        public static T CreateInstance<T>(GameObject obj) where T : class
        {
            if (!obj)
                return null;
            
            var component = obj.AddComponent(typeof(T)) as T;
            // 注入字段/属性
            InjectIntoInstance(component);
            return component;
        }

        /// <summary>
        /// 通过构造函数创建实例
        /// </summary>
        /// <param name="type">必须是实例类型</param>
        /// <param name="explicitArgs">构造函数参数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static object CreateInstanceWithConstructorInjection(Type type, ParameterArg[] explicitArgs)
        {
            if (type.IsInterface)
                throw new ArgumentException($"Type is not an interface，{type}");
            
            // 临时字典，参数名称到值的映射，解决多线程同时创建实例问题（线程安全字典）
             ConcurrentDictionary<string, object> _argMap = new();
            // 将显式参数转换为按参数名称索引类型的字典
            foreach (var kv in explicitArgs)
            {
                _argMap.TryAdd(kv.ArgName, kv.ArgValue);
            }

            // 获取所有的构造函数
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // 按参数数量降序排序（优先匹配参数最多的构造函数）
            Array.Sort(constructors, (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));
            // 遍历所有构造函数
            foreach (var ctor in constructors)
            {
                // 获取当前构造所有参数
                var parameters = ctor.GetParameters();
                var args = new object[parameters.Length];
                var allResolved = true;

                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var paramName = parameters[i].Name;
                    // 优先使用显式参数中名称匹配的值
                    if (_argMap.TryGetValue(paramName, out var value))
                    {
                        args[i] = value;
                        continue;
                    }

                    // 尝试从容器中获取依赖
                    value = Resolve(paramType);
                    if (value != null)
                    {
                        args[i] = value;
                        continue;
                    }

                    // 如果是可选参数，使用默认值
                    if (parameters[i].IsOptional)
                    {
                        args[i] = parameters[i].DefaultValue;
                        continue;
                    }

                    // 无法解析，这个构造函数不可用，用下一个构造函数再次尝试
                    allResolved = false;
                    break;
                }

                // 找到可用构造，创建实例
                if (allResolved)
                {
                    return ctor.Invoke(args);
                }
            }

            // 如果没有合适的构造函数，尝试无参构造
            var defaultCtor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            return defaultCtor != null ? defaultCtor.Invoke(null) : throw new Exception($"Cannot create instance of {type.Name}: no suitable constructor found");
        }
        
        /// <summary>
        /// 注入依赖到实例的被Inject修饰的字段/属性
        /// </summary>
        /// <param name="instance">类型实例</param>
        public static void InjectIntoInstance(object instance)
        {
            var type = instance.GetType();
            // 注入字段
            var fields = type.GetFields(_bindingFlags);
            foreach (var field in fields)
            {
                // 跳过标记为过时的字段
                if(field.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                // 字段有值，则是上一步构造赋值，跳过即可
                if (field.GetValue(instance) != null) continue;
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;

                var value = Resolve(field.FieldType);
                if (value != null)
                {
                    field.SetValue(instance, value);
                }
                else
                {
                    Debug.Log($"{type}的字段 {field.FieldType} 未找到依赖项");
                }
            }
        
            // 注入属性
            var properties = type.GetProperties(_bindingFlags);
            foreach (var property in properties)
            {
                // 跳过标记为过时的属性
                if(property.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                // 字属性有值，则是上一步构造赋值，跳过即可
                if (property.CanRead && property.GetValue(instance) != null) continue;
                if (!Attribute.IsDefined(property, typeof(InjectAttribute))) continue;
                if (!property.CanWrite) continue;
            
                var value = Resolve(property.PropertyType);
                if (value != null)
                {
                    property.SetValue(instance, value);
                }
                else
                {
                    Debug.Log($"{type}的属性 {property.PropertyType} 未找到依赖项");
                }
            }
        }

        public static Task InitAsync()
        {
            List<IApplicationExitNotify> notifies = new(_interfaceMap.Values.ToArray(obj => obj as IApplicationExitNotify));
            SingletonInitializer.InitQuit(GetInstance<IMonoAdapter>(), notifies);
            // 初始化单例
            List<IInitializable> initializers = new(_interfaceMap.Values.ToArray(obj => obj as IInitializable));
            return SingletonInitializer.InitAsync(initializers);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T">传入实例实现的接口类型</typeparam>
        /// <returns></returns>
        public static T GetInstance<T>() where T : class
        {
            if (_interfaceMap.ContainsKey(typeof(T))) return _interfaceMap[typeof(T)] as T;
            return null;
        }
        
        /// <summary>
        /// 移除依赖缓存，传入具体类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveDependency<T>()
        {
            return _interfaceMap.Remove(typeof(T));
        }

        /// <summary>
        /// 情况所有缓存
        /// </summary>
        public static void Clear()
        {
            _interfaceMap.Clear();
        }
    }
}
