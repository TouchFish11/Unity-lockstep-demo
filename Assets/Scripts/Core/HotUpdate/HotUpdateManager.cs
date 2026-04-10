using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Serialize.Json;
using Core.Singleton;
using HybridCLR;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Core.HotUpdate
{
    /// <summary>
    /// 热更新管理器
    /// </summary>
    public class HotUpdateManager : IHotUpdateManager, IInitializable
    {
        public int InitPriority => 2;
        // 缓存热更程序集名称
        private readonly ConcurrentBag<string> _assemblyNames = new();
        // 热更新程序集设置
        private HotUpdateAssemblySettings _hotupdateassemblySettings;
        [Inject] private IAssetBundleManager _assetBundleManager;
        [Inject] private IJsonManager _jsonManager;
        
        private HotUpdateManager(){}

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public void LoadMetadataForAOTAssemblies(IReadOnlyList<string> aotDlls)
        {
            foreach (var aotDllName in aotDlls)
            {
                var assemblyBytes = GetAssemblyBytes(aotDllName);
                var errorCode = RuntimeApi.LoadMetadataForAOTAssembly(assemblyBytes, HomologousImageMode.SuperSet);
                Logger.Log($"{nameof(HotUpdateManager)}.{nameof(LoadMetadataForAOTAssemblies)}:已补充元数据{aotDllName}，错误码:{errorCode}");
            }
        }

        public async Task PreLoadAssembliesAsync(string abName)
        {
            // 加载热更新AB包资源
            var batchHandle = await GameAsset.LoadAssetsAsync<TextAsset>();
            
            var textAsset = batchHandle.Assets.Find(text => text.name.Contains(nameof(HotUpdateAssemblySettings)));
            if (textAsset)
            {
                _hotupdateassemblySettings = _jsonManager.FromJson<HotUpdateAssemblySettings>(textAsset.text);
                if (_hotupdateassemblySettings != null)
                {
                    Logger.Log($"{nameof(HotUpdateManager)}.{nameof(PreLoadAssembliesAsync)}:内容长度{_hotupdateassemblySettings.preloadHotUpdateAssemblies.Length}");
                }
                else
                {
                    Logger.LogWarning($"{nameof(HotUpdateManager)}.{nameof(PreLoadAssembliesAsync)}:HotUpdateAssemblySettings反序列化失败");
                    return;
                }
            }
            else
            {
                Logger.LogWarning($"{nameof(HotUpdateManager)}.{nameof(PreLoadAssembliesAsync)}:HotUpdateAssemblySettings文件未找到");
                return;
            }
            
            // 顺序加载程序集资源
            foreach (var nameWithExtension in _hotupdateassemblySettings.preloadHotUpdateAssemblies)
            {
                foreach (var dllText in batchHandle.Assets)
                {
                    if (nameWithExtension != dllText.name) continue;
                    // 多线程加载程序集
                    await LoadAssemblyAsyncInternal(dllText.bytes);
                    break;
                }
            }

            GameAsset.Release(batchHandle);
        }

        public async Task LoadAssembliesAsync(string abName)
        {
            try
            {
                // 加载热更新AB包资源
                var batchHandle = await GameAsset.LoadAssetsAsync<TextAsset>();
            
                foreach (var dllText in batchHandle.Assets)
                {
                    if (dllText.name == nameof(HotUpdateAssemblySettings)) continue;
                    if (_assemblyNames.Contains(dllText.name[..dllText.name.LastIndexOf('.')])) continue;
                    // 多线程加载程序集
                    await LoadAssemblyAsyncInternal(dllText.bytes);
                }
            
                _assetBundleManager.UnloadBundle(abName);
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(HotUpdateManager)}.{nameof(LoadAssembliesAsync)}:{e.Message}");   
            }
        }

        public Assembly GetAssembly(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
        
        public Assembly GetCoreModule()
        {
            return Assembly.Load("CoreModule");
        }

        public Assembly GetGameModule()
        {
            return Assembly.Load("GameModule");
        }
        
        /// <summary>
        /// 获取所有程序集
        /// </summary>
        /// <returns></returns>
        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>
            {
                GetCoreModule(),
                GetGameModule()
            };
            
            // 获取所有热更后的程序集
            assemblies.AddRange(GetHotAssemblies()); 
            return assemblies.ToArray();
        }
        
        /// <summary>
        /// 获取所有程序集
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public int GetAssemblies(List<Assembly> assemblies)
        {
            assemblies.Add(GetCoreModule());
            assemblies.Add(GetGameModule());
            // 获取所有热更后的程序集
            assemblies.AddRange(GetHotAssemblies()); 
            return assemblies.Count;
        }
        
        public Assembly[] GetHotAssemblies()
        {
            var assemblies = new List<Assembly>(_assemblyNames.Count);
            foreach (var assemblyName in _assemblyNames)
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }
            return assemblies.ToArray();
        }
        
        /// <summary>
        /// 获取所有热更程序集
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public int GetHotAssemblies(List<Assembly> assemblies)
        {
            foreach (var assemblyName in _assemblyNames)
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }
            return assemblies.Count;
        }

        /// <summary>
        /// 异步加载程序集
        /// </summary>
        /// <param name="bytes">程序集字节数组</param>
        /// <returns></returns>
        private Task LoadAssemblyAsyncInternal(byte[] bytes)
        {
            return Task.Run(() =>
            {
                try
                {
                    var assembly = Assembly.Load(bytes);
                    _assemblyNames.Add(assembly.GetName().Name);
                    Logger.Log($"{nameof(HotUpdateManager)}.{nameof(LoadAssemblyAsyncInternal)}:已加载热更程序集{assembly.GetName().Name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(HotUpdateManager)}.{nameof(LoadAssemblyAsyncInternal)}:热更程序集加载错误{e.Message}");
                    Logger.LogError($"{nameof(HotUpdateManager)}.{nameof(LoadAssemblyAsyncInternal)}:热更程序集加载错误{e.Message}");
                }
            });
        }
        
        /// <summary>
        /// TODO：补充的程序集单独打包AB包加载
        /// 获取程序集字节数组
        /// </summary>
        /// <param name="assemblyNameWithExtension">包含拓展名的程序集名称</param>
        /// <returns></returns>
        private static byte[] GetAssemblyBytes(string assemblyNameWithExtension)
        {
            return File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, $"{assemblyNameWithExtension}.bytes"));
        }
    }
}
