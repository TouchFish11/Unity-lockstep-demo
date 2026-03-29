using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.AssetBundles.Update.Core;
using Core.EditorRes;
using Core.Extensions;
using Core.Global;
using Core.GlobalEvent;
using Core.HotUpdate;
using Core.Input.ActionAsset;
using Core.Mono;
using Core.Music;
using Core.Net;
using Core.Pool;
using Core.PreLoad;
using Core.Reflection;
using Core.Res;
using Core.Scene;
using Core.ScriptableObject;
using Core.Serialize.Binary;
using Core.Serialize.Json;
using Core.Singleton;
using Core.Systems.Memorys;
using Core.Time;
using Core.UI;
using Core.Video;
using UnityEngine;

namespace Core.DI
{
    /// <summary>
    /// 依赖容器
    /// </summary>
    public class DIContainer
    {
        private static readonly Dictionary<Type, object> _dependencies = new();
        // 绑定标志
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        // 初始化器
        private static readonly SingletonInitializer _initializer = new();
        
        /// <summary>
        /// 创建类型单例
        /// </summary>
        /// <param name="isMono">该单例是否是monoBehaviour</param>
        /// <typeparam name="T">作为单例的类型</typeparam>
        public static void BindSingleton<T>(bool isMono = false) where T : class
        {
            if (_dependencies.ContainsKey(typeof(T))) return;
            
            if (isMono)
            {
                var monoSingleton =  new GameObject(typeof(T).ToString());
                var t = monoSingleton.AddComponent(typeof(T));
                _dependencies.Add(typeof(T), t);
            }
            else
            {
                var constructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructorInfo == null) throw new ArgumentException($"{typeof(T)} does not have a parameterless constructor.");
                _dependencies.Add(typeof(T), constructorInfo.Invoke(null));
            }
        }

        /// <summary>
        /// 注入实例，用于后续注入的实例，可以其它依赖通过GetDependency主动获取。相同类型只能对应唯一实例
        /// </summary>
        /// <param name="instance">类型实例</param>
        /// <typeparam name="T">实例类型</typeparam>
        public static void InjectInstance<T>(T instance)
        {
            _dependencies.TryAdd(typeof(T), instance);
        }
        
        /// <summary>
        /// 注入依赖项，遍历所有注入的类型单例，注入其各自的依赖项。在所有BindSingleton方法调用完后执行
        /// </summary>
        public static void InjectDependencies()
        {
            // 注入实例
            foreach (var instance in _dependencies.Values)
            {
                var fieldInfos = instance.GetType().GetFields(_bindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    if(!Attribute.IsDefined(fieldInfo, typeof(InjectAttribute))) continue;

                    if (fieldInfo.FieldType.IsInterface)
                    {
                        var value = GetValueByType(fieldInfo.FieldType);
                        if (value == null) continue;
                        fieldInfo.SetValue(instance, value);
                    }
                    else
                    {
                        var value = _dependencies.GetValueOrDefault(fieldInfo.FieldType);
                        if (value == null) continue;
                        fieldInfo.SetValue(instance, value);
                    }
                }
            }
        }

        private static object GetValueByType(Type type)
        {
            foreach (var kvp in _dependencies)
            {
                if(!kvp.Key.IsAssignableFrom(type)) continue;
                return kvp.Value;
            }

            return null;
        }

        public static Task InitAsync()
        {
            List<IInitializable> initializers = new(_dependencies.Values.ToArray(obj => obj as IInitializable));
            List<IApplicationExitNotify> notifies = new(_dependencies.Values.ToArray(obj => obj as IApplicationExitNotify));
            SingletonInitializer.InitQuit(GetInstance<IMonoAdapter>(), notifies);
            // 初始化单例
            return SingletonInitializer.InitAsync(initializers);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T">传入实例实现的接口类型</typeparam>
        /// <returns></returns>
        public static T GetInstance<T>() where T : class
        {
            if (!_dependencies.ContainsKey(typeof(T)))
            {
                var singletonBase = new SingletonBase<T>();
                var instance = singletonBase.Instance;
                _dependencies.Add(typeof(T), instance);
                return instance;
            }
            
            foreach (var kvp in _dependencies)
            {
                if (!kvp.Key.IsAssignableFrom(typeof(T))) continue;
                return (T)kvp.Value;
            }

            return null;
        }
        
        /// <summary>
        /// 移除依赖缓存，传入具体类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveDependency<T>()
        {
            return _dependencies.Remove(typeof(T));
        }

        /// <summary>
        /// 情况所有缓存
        /// </summary>
        public static void Clear()
        {
            _dependencies.Clear();
        }

        public static void RegisterSingletons()
        {
            BindSingleton<MonoAdapter>(true);
            BindSingleton<MemoryMonitor>();
            BindSingleton<UWRManager>();
            BindSingleton<PoolManager>();
            BindSingleton<UIManager>();
            BindSingleton<AssetBundleManager>();
            BindSingleton<AssetBundleUpdater>();
            BindSingleton<BinaryDataManager>();
            BindSingleton<EditorResManager>();
            BindSingleton<EventCenter>();
            BindSingleton<InputSystem>();
            BindSingleton<JsonManager>();
            BindSingleton<MusicManager>();
            BindSingleton<ResourcesManager>();
            BindSingleton<ScriptableObjectManager>();
            BindSingleton<TimerManager>();
            BindSingleton<VideoManager>();
            BindSingleton<FactoryManager>(); 
#if UNITY_EDITOR
            BindSingleton<HotUpdateMockManager>();
#else
            BindSingleton<HotUpdateManager>();
#endif
            BindSingleton<SceneManager>();
            BindSingleton<PreLoadManager>();
            BindSingleton<GameSettingManager>();
        }
    }
}
