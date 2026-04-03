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
using Core.Serialize.Binary;
using Core.Serialize.Json;
using Core.Singleton;
using Core.Systems.Memorys;
using Core.Time;
using Core.UI;
using Core.Video;
using UnityEngine;
using ILogger = Core.Log.ILogger;
using Logger = Core.Log.Logger;

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

        /// <summary>
        /// 创建类型单例
        /// </summary>
        /// <param name="isMono">该单例是否是monoBehaviour</param>
        /// <typeparam name="TInstance">作为单例的类型</typeparam>
        /// <typeparam name="TInterface">类型接口</typeparam>
        public static void BindSingleton<TInterface, TInstance>(bool isMono = false) where TInterface : class where TInstance : TInterface
        {
            if (_dependencies.ContainsKey(typeof(TInstance))) return;
            
            if (isMono)
            {
                var monoSingleton =  new GameObject(typeof(TInstance).ToString());
                var t = monoSingleton.AddComponent(typeof(TInstance));
                _dependencies.Add(typeof(TInterface), t);
            }
            else
            {
                var constructorInfo = typeof(TInstance).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (constructorInfo == null) throw new ArgumentException($"{typeof(TInstance)} does not have a parameterless constructor.");
                _dependencies.Add(typeof(TInterface), constructorInfo.Invoke(null));
            }
        }

        /// <summary>
        /// 注入实例，用于后续注入的实例，可以其它依赖通过GetInstance主动获取。一个接口对应唯一实例
        /// </summary>
        /// <param name="instance">类型实例</param>
        /// <typeparam name="TInterface">实例接口类型</typeparam>
        public static void InjectInstance<TInterface>(TInterface instance) where TInterface : class
        {
            _dependencies.TryAdd(typeof(TInterface), instance);
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
            List<IApplicationExitNotify> notifies = new(_dependencies.Values.ToArray(obj => obj as IApplicationExitNotify));
            SingletonInitializer.InitQuit(GetInstance<IMonoAdapter>(), notifies);
            // 初始化单例
            List<IInitializable> initializers = new(_dependencies.Values.ToArray(obj => obj as IInitializable));
            return SingletonInitializer.InitAsync(initializers);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T">传入实例实现的接口类型</typeparam>
        /// <returns></returns>
        public static T GetInstance<T>() where T : class
        {
            if (_dependencies.ContainsKey(typeof(T))) return _dependencies[typeof(T)] as T;
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
            BindSingleton<IMonoAdapter, MonoAdapter>(true);
            BindSingleton<ILogger, Logger>();
            BindSingleton<IMemoryMonitor, MemoryMonitor>();
            BindSingleton<IUWRManager, UWRManager>();
            BindSingleton<IPoolManager, PoolManager>();
            BindSingleton<IUIManager, UIManager>();
            BindSingleton<IAssetBundleManager, AssetBundleManager>();
            BindSingleton<IAssetBundleUpdater, AssetBundleUpdater>();
            BindSingleton<IBinaryDataManager, BinaryDataManager>();
            BindSingleton<IEditorResManager, EditorResManager>();
            BindSingleton<IEventCenter, EventCenter>();
            BindSingleton<IInputSystem, InputSystem>();
            BindSingleton<IJsonManager, JsonManager>();
            BindSingleton<IMusicManager, MusicManager>();
            BindSingleton<IResourcesManager, ResourcesManager>();
            BindSingleton<ITimerManager, TimerManager>();
            BindSingleton<IVideoManager, VideoManager>();
            BindSingleton<IFactoryManager, FactoryManager>(); 
#if UNITY_EDITOR
            BindSingleton<IHotUpdateManager, HotUpdateMockManager>();
#else
            BindSingleton<IHotUpdateManager, HotUpdateManager>();
#endif
            BindSingleton<ISceneManager, SceneManager>();
            BindSingleton<IPreLoadManager, PreLoadManager>();
            BindSingleton<IGameSettingManager, GameSettingManager>();
        }
    }
}
