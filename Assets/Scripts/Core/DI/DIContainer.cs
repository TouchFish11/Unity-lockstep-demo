using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Mono;
using Core.Singleton;
using UnityEngine;

namespace Core.DI
{
    /// <summary>
    /// 依赖容器
    /// </summary>
    public class DIContainer
    {
        // 接口类型映射
        private static readonly Dictionary<Type, object> _interfaceMap = new();
        // 实例类型映射
        private static readonly Dictionary<Type, object> _instanceMap = new();
        // 绑定标志
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        // 临时字典，参数名称到值的映射，解决多线程同时创建实例问题（线程安全字典）
        private static readonly ConcurrentDictionary<string, object> _argMap = new();
        
        /// <summary>
        /// 创建类型单例
        /// </summary>
        /// <param name="isMono">该单例是否是monoBehaviour</param>
        /// <typeparam name="TInstance">作为单例的类型</typeparam>
        /// <typeparam name="TInterface">类型接口</typeparam>
        public static void BindSingleton<TInterface, TInstance>(bool isMono = false) where TInterface : class where TInstance : class, TInterface
        {
            if (_interfaceMap.ContainsKey(typeof(TInstance))) return;
            
            if (isMono)
            {
                var monoSingleton =  new GameObject(typeof(TInstance).ToString());
                var t = monoSingleton.AddComponent(typeof(TInstance));
                _interfaceMap.Add(typeof(TInterface), t);
                _instanceMap.Add(typeof(TInstance), t);
            }
            else
            {
                var constructorInfo = typeof(TInstance).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructorInfo == null) throw new ArgumentException($"{typeof(TInstance)} does not have a parameterless constructor.");
                var instance = constructorInfo.Invoke(null);
                _interfaceMap.Add(typeof(TInterface), instance);
                _instanceMap.Add(typeof(TInstance), instance);
            }
        }

        /// <summary>
        /// 通过反射创建实例。先尝试构造函数注入（选择参数都能解析的构造函数），再对标记了[Inject]的字段/属性进行补充注入
        /// </summary>
        /// <param name="isSingleton">是否是单例，true创建为单例，false则是瞬态对象</param>
        /// <param name="constructorArgs">构造参数</param>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns>新类型实例</returns>
        /// <exception cref="ArgumentException">重复创建单例类型则抛出异常</exception>
        public static T Create<T>(bool isSingleton = false, params object[] constructorArgs) where T : class
        {
            // 通过构造函数创建实例
            var instance = CreateInstanceWithConstructorInjection(typeof(T), constructorArgs);
            // 注入字段/属性
            InjectIntoInstance(instance);
            // 不是单例直接返回
            if (!isSingleton) return (T)instance;
            return _instanceMap.TryAdd(typeof(T), instance) ? (T)instance : throw new ArgumentException($"{typeof(T)} already exists.");
        }

        /// <summary>
        /// 通过反射创建实例。先尝试构造函数注入（选择参数都能解析的构造函数），再对标记了[Inject]的字段/属性进行补充注入
        /// </summary>
        /// <param name="type">实例类型</param>
        /// <param name="isSingleton">是否是单例，true创建为单例，false则是瞬态对象</param>
        /// <param name="constructorArgs">构造参数</param>
        /// <returns>新类型实例</returns>
        /// <exception cref="ArgumentException">重复创建单例类型则抛出异常</exception>
        public static object Create(Type type, bool isSingleton = false, params object[] constructorArgs)
        {
            // 通过构造函数创建实例
            var instance = CreateInstanceWithConstructorInjection(type, constructorArgs);
            // 注入字段/属性
            InjectIntoInstance(instance);
            // 不是单例直接返回
            if (!isSingleton) return instance;
            return _instanceMap.TryAdd(type, instance) ? instance : throw new ArgumentException($"{type} already exists.");
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
        /// <param name="type"></param>
        /// <param name="explicitArgs"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static object CreateInstanceWithConstructorInjection(Type type, object[] explicitArgs)
        {
            _argMap.Clear();
            // 将显式参数转换为按参数名称索引类型的字典
            foreach (var arg in explicitArgs)
            {
                if (arg != null)
                {
                    _argMap.TryAdd(arg.GetType().Name, arg);
                }
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
                    // 优先使用显式参数中类型匹配的值
                    if (_argMap.TryGetValue(paramType.Name, out var value))
                    {
                        args[i] = value;
                        continue;
                    }

                    // 尝试从容器中获取依赖
                    value = _interfaceMap.GetValueOrDefault(paramType) ?? _instanceMap.GetValueOrDefault(paramType);
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
                // 字段有值，则是上一步构造赋值，跳过即可
                if (field.GetValue(instance) != null) continue;
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;

                var value = _interfaceMap.GetValueOrDefault(field.FieldType) ?? _instanceMap.GetValueOrDefault(field.FieldType);
                if (value != null)
                {
                    field.SetValue(instance, value);
                }
            }
        
            // 注入属性
            var properties = type.GetProperties(_bindingFlags);
            foreach (var property in properties)
            {
                // 字属性有值，则是上一步构造赋值，跳过即可
                if (property.CanRead && property.GetValue(instance) != null) continue;
                if (!Attribute.IsDefined(property, typeof(InjectAttribute))) continue;
                if (!property.CanWrite) continue;
            
                var value = _interfaceMap.GetValueOrDefault(property.PropertyType) ?? _instanceMap.GetValueOrDefault(property.PropertyType);
                if (value != null)
                {
                    property.SetValue(instance, value);
                }
            }
        }
        
        /// <summary>
        /// 注入依赖项，遍历所有注入的类型单例，注入其各自的依赖项。在所有BindSingleton方法调用完后执行
        /// </summary>
        public static void InjectDependencies()
        {
            // 注入字段实例
            foreach (var instance in _interfaceMap.Values)
            {
                InjectIntoInstance(instance);
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

        //         public static void RegisterSingletons()
        //         {
        //             BindSingleton<IMonoAdapter, MonoAdapter>(true);
        //             BindSingleton<ILogger, Logger>();
        //             BindSingleton<IMemoryMonitor, MemoryMonitor>();
        //             BindSingleton<IUWRManager, UWRManager>();
        //             BindSingleton<IPoolManager, PoolManager>();
        //             BindSingleton<IUIManager, UIManager>();
        //             BindSingleton<IAssetBundleManager, AssetBundleManager>();
        //             BindSingleton<IAssetBundleUpdater, AssetBundleUpdater>();
        //             BindSingleton<IBinaryDataManager, BinaryDataManager>();
        //             BindSingleton<IEditorResManager, EditorResManager>();
        //             BindSingleton<IEventCenter, EventCenter>();
        //             BindSingleton<IInputSystem, InputSystem>();
        //             BindSingleton<IJsonManager, JsonManager>();
        //             BindSingleton<IMusicManager, MusicManager>();
        //             BindSingleton<IResourcesManager, ResourcesManager>();
        //             BindSingleton<ITimerManager, TimerManager>();
        //             BindSingleton<IVideoManager, VideoManager>();
        //             BindSingleton<IFactoryManager, FactoryManager>(); 
        // #if UNITY_EDITOR
        //             BindSingleton<IHotUpdateManager, HotUpdateMockManager>();
        // #else
        //             BindSingleton<IHotUpdateManager, HotUpdateManager>();
        // #endif
        //             BindSingleton<ISceneManager, SceneManager>();
        //             BindSingleton<IPreLoadManager, PreLoadManager>();
        //         }
    }
}
