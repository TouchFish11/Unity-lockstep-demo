using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.HotUpdate;
using Core.Registration;
using Core.Serialize.Json;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Game.Main
{
    /// <summary>
    /// 游戏启动器
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [SerializeField] private string bootConfigFileName = "BootConfig.json";
        
        private async void Start()
        {
            try
            {
                // 注册框架
                await RegisterCore.InitCore();
                // 加载热更程序集
                await LoadHotfixDll();
                // 创建热更入口
                var spawner = DIContainer.Create<ObjectSpawner>();
                var entryObj = await spawner.SpawnAsync<GameObject>("HotUpdateEntry");
                entryObj.Collect();
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(GameLauncher)}:Game startup failed {e.Message})");
            }
        }

        /// <summary>
        /// 加载热更程序集
        /// </summary>
        private async Task LoadHotfixDll()
        {
            // 加载启动配置
            var bootConfig = await LoadBootConfigAsync();
            if (bootConfig == null)
            {
                Logger.LogError($"{nameof(GameLauncher)}:无法加载启动配置，使用默认硬编码包名");
                bootConfig = new BootConfig { hotfixDllBundleName = "hotupdate.assetbundle" };
            }
            
            // 加载所有dll资源
            var handle = await GameAsset.LoadAllAssetByBundleAsync<TextAsset>(bootConfig.hotfixDllBundleName);
            var list = new List<TextAsset>(handle.Asset);
            // 获取热更程序集依赖设置
            var settingsTextAsset = list.Find(text => text.name.Contains(nameof(HotUpdateAssemblySettings)));
            list.Remove(settingsTextAsset);
            var settings = DIContainer.Create<JsonManager>().FromJson<HotUpdateAssemblySettings>(settingsTextAsset.text);
            var hotUpdateManager = DIContainer.GetInstance<IHotUpdateManager>();
            // 补充元数据
            hotUpdateManager.LoadMetadataForAOTAssemblies(AOTGenericReferences.PatchedAOTAssemblyList);  
            // 加载所有热更程序集
            await hotUpdateManager.LoadAssembliesAsync(settings, list);
        }
        
        private async Task<BootConfig> LoadBootConfigAsync()
        {
            var jsonManager = DIContainer.GetInstance<IJsonManager>();
            // 优先从持久化目录读取（热更可能更新配置，但通常不需要）
            var persistentPath = Path.Combine(Application.persistentDataPath, bootConfigFileName);
            if (File.Exists(persistentPath))
            {
                var json = await File.ReadAllTextAsync(persistentPath);
                return jsonManager.FromJson<BootConfig>(json);
            }

            // 其次从 StreamingAssets 读取（首包内置）
            var streamingPath = Path.Combine(Application.streamingAssetsPath, bootConfigFileName);
            if (File.Exists(streamingPath))
            {
                var json = await File.ReadAllTextAsync(streamingPath);
                return jsonManager.FromJson<BootConfig>(json);
            }

            return null;
        }
    }
}
