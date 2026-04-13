using System;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.HotUpdate;
using Core.Singleton;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Game
{
    /// <summary>
    /// 主入口
    /// </summary>
    public class Main : SingletonMono<Main>
    {
        // 默认包名称数组
        private readonly string[] DefaultAbNames = { "default", "fonts", "tmp_asset", "hotupdate" };
        
        /// <summary>
        /// 游戏启动入口
        /// </summary>
        private async void Start()
        {
            try
            {
                // 初始化游戏设置
                InitSettings();
                InitDI();
                // 初始化指定AB包
                await DIContainer.GetInstance<IAssetBundleManager>().InitSpecifyAsync(DefaultAbNames);
                var hotUpdateManager = DIContainer.GetInstance<IHotUpdateManager>();
                // 补充元数据
                hotUpdateManager.LoadMetadataForAOTAssemblies(AOTGenericReferences.PatchedAOTAssemblyList);  
                // 加载指定程序集
                await hotUpdateManager.PreLoadAssembliesAsync(DefaultAbNames[3]);
                // 实例化热更入口对象
                var handle = await GameAsset.LoadAssetAsync<GameObject>("HotUpdateEntry");
                Instantiate(handle.Asset);
                GameAsset.Release(handle);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(Main)}.{nameof(Start)}: 游戏启动错误，{e.Message}");
            }
        }

        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <returns></returns>
        private static void InitDI()
        {
            // // 注册框架单例
            //DIContainer.RegisterSingletons();
        }
        
        /// <summary>
        /// 初始化设置
        /// </summary>
        private static void InitSettings()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
        }
    }
}
