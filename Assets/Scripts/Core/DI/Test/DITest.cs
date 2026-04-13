using Core.AssetBundles.Management;
using Core.AssetBundles.Update.Core;
using Core.Collection;
using Core.DI.Test.Services;
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
        [SerializeField] private SerializableDictionary<string, int> _serializableDictionary;
        
        private class GameLaunch
        {
            [Inject] private IMemoryMonitor _memoryMonitor;
            [Inject] private IMonoAdapter _monoAdapter;
            [Inject] private ITimerManager _timerManager;
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            // 绑定框架
            BindSingletons();
            Log.Logger.Log($"初始化依赖成功");
            
            // 绑定业务类型
            // 创建业务层的管理器单例
            var bagManager = DIContainer.Create<BagManager>(true);
            bagManager.Test();
            
            MainTest();
        }

        private void MainTest()
        {
            DIContainer.Create<GameLaunch>();
            
            SerializableDictionary<string, int> serializableDictionary = new();
        }
        
        private static void BindSingletons()
        {
            DIContainer.BindSingleton<IMonoAdapter, MonoAdapter>();
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
            DIContainer.BindSingleton<IVideoManager, VideoPlayManager>();
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
