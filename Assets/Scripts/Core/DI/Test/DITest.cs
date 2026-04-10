using Core.AssetBundles.Management;
using Core.AssetBundles.Update.Core;
using Core.EditorRes;
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
using Core.Systems.Memorys;
using Core.Time;
using Core.UI;
using Core.Video;
using UnityEngine;

namespace Core.DI.Test
{
    public class DITest : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            RegisterSingleton();
            
            DIContainer.InjectDependencies();
        
            // DIContainer.GetInstance<ServiceA>().DoSomething();
            // DIContainer.GetInstance<ServiceB>().DoSomething();
            // DIContainer.GetInstance<FactoryC>().DoSomething();
        }
        
        private static void RegisterSingleton()
        {
            DIContainer.BindSingleton<IMonoAdapter, MonoAdapter>(true);
            DIContainer.BindSingleton<ILogger, Logger>();
            DIContainer.BindSingleton<IMemoryMonitor, MemoryMonitor>();
            DIContainer.BindSingleton<IUWRManager, UWRManager>();
            DIContainer.BindSingleton<IPoolManager, PoolManager>();
            DIContainer.BindSingleton<IUIManager, UIManager>();
            DIContainer.BindSingleton<IAssetBundleManager, AssetBundleManager>();
            DIContainer.BindSingleton<IAssetBundleUpdater, AssetBundleUpdater>();
            DIContainer.BindSingleton<IBinaryDataManager, BinaryDataManager>();
            DIContainer.BindSingleton<IEditorResManager, EditorResManager>();
            DIContainer.BindSingleton<IEventCenter, EventCenter>();
            DIContainer.BindSingleton<IInputSystem, InputSystem>();
            DIContainer.BindSingleton<IJsonManager, JsonManager>();
            DIContainer.BindSingleton<IMusicManager, MusicManager>();
            DIContainer.BindSingleton<IResourcesManager, ResourcesManager>();
            DIContainer.BindSingleton<ITimerManager, TimerManager>();
            DIContainer.BindSingleton<IVideoManager, VideoManager>();
            DIContainer.BindSingleton<IFactoryManager, FactoryManager>(); 
#if UNITY_EDITOR
            DIContainer.BindSingleton<IHotUpdateManager, HotUpdateMockManager>();
#else
            DIContainer.BindSingleton<IHotUpdateManager, HotUpdateManager>();
#endif
            DIContainer.BindSingleton<ISceneManager, SceneManager>();
            DIContainer.BindSingleton<IPreLoadManager, PreLoadManager>();
        }
    }
}
