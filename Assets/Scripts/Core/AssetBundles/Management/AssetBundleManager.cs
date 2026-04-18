using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Serialize.Json;
using Core.Systems.Memorys;
using Core.Utility;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// AB包管理器
    /// </summary>
    internal class AssetBundleManager : IAssetBundleManager
    {
        private readonly IJsonManager _jsonManager;
        // 缓存包包装器，便于查找
        private readonly Dictionary<string, BundleWrapper> _nameToWrapperMap = new();
        // 
        private readonly List<BundleWrapper> _hotBundles = new();
        //
        private readonly List<BundleWrapper> _coldBundles = new();
        /// 临界活跃数，高于该数值则放入热包列表，小于则放入冷包列表
        private const int CriticalActiveCount = 2;
        
        public AssetCatalog Catalog { get; private set; }
        
        private AssetBundleManager(IMemoryMonitor memoryMonitor, IJsonManager jsonManager)
        {
            // 注册事件
            memoryMonitor.Register(this);
            _jsonManager = jsonManager;
        }
        
        public async Task Init()
        {
            // 读取本地清单文件
            Catalog = await _jsonManager.FromJsonAsync<AssetCatalog>(PathUtility.GetAbLoadPath(FileUtility.CatalogDefaultName));
            // 构建全部AB包信息
            foreach (var abPackageInfo in Catalog.ABPackageCollection.Values)
            {
                var abName = abPackageInfo.Name;
                // 初始化包装器
                _nameToWrapperMap.TryAdd(abName, new BundleWrapper(abName, PathUtility.GetAbLoadPath($"{abPackageInfo.Name}{FileUtility.AbSuffix}"), this));
            }
        }
        
        public async Task<BundleWrapper> LoadBundleAsync(string abName, CancellationToken token = default)
        {
            if (!_nameToWrapperMap.TryGetValue(abName, out var wrapper))
            {
                throw new KeyNotFoundException($"{nameof(AssetBundleManager)}: {abName} key is not found");
            }

            // 加载依赖和目标AB包
            await LoadDependenciesAndTargetAsync(abName, token);
            // 返回指定AB包
            return wrapper;
        }

        /// <summary>
        /// 异步加载依赖包和目标包
        /// </summary>
        /// <param name="abName">AB包名称（不含拓展名）</param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task LoadDependenciesAndTargetAsync(string abName, CancellationToken token)
        {
            // 获取该AB包的所有依赖
            var dependencies = Catalog.ABPackageCollection.GetAllDependencies(abName);
            // 加载所有依赖包
            foreach (var dependency in dependencies)
            {
                var wrapper = _nameToWrapperMap[dependency];
                wrapper.IsActive = true;
                await wrapper.LoadFromFileAsync(token);
                Logger.Log($"{nameof(AssetBundleManager)}.{nameof(LoadDependenciesAndTargetAsync)}：{abName}包依赖项{dependency}已加载");
            }

            // 加载目标包
            await _nameToWrapperMap[abName].LoadFromFileAsync(token);
        }

        public void ReleaseDependencies(string abName)
        {
            var dependencies = Catalog.ABPackageCollection.GetAllDependencies(abName);
            foreach (var dependency in dependencies)
            {
                var wrapper = _nameToWrapperMap[dependency];
                if (wrapper.IsActive)
                {
                    wrapper.Release();
                }
            }
        }

        /// <summary>
        /// 卸载所有已加载的AssetBundle
        /// 调用该方法后，若需要加载AB包，需重新初始化（Init）管理器
        /// </summary>
        /// <param name="unloadAllObjects"></param>
        public async Task UnloadAllBundles(bool unloadAllObjects)
        {
            foreach (var bundleWrapper in _nameToWrapperMap.Values)
            {
                await bundleWrapper.TryUnloadAsync(unloadAllObjects);
                if (unloadAllObjects)
                {
                    if (bundleWrapper.RefCount != 0)
                    {
                        Logger.LogWarning($"{nameof(AssetBundleManager)}.{nameof(UnloadAllBundles)}:{bundleWrapper.BundleName}包和已加载资源已卸载，剩余引用计数{bundleWrapper.RefCount}，可能导致引用丢失");
                    }
                }
                else
                {
                    Logger.Log($"{nameof(AssetBundleManager)}.{nameof(UnloadAllBundles)}:{bundleWrapper.BundleName}包已卸载，剩余引用计数{bundleWrapper.RefCount}");
                }
            }
            
            // 清空缓存
            _nameToWrapperMap.Clear();
            // 置空清单集合
            Catalog = null;
            // 卸载所有AB包
            AssetBundle.UnloadAllAssetBundles(unloadAllObjects);
            GC.Collect();
        }
        
        public async void OnReport()
        {
            try
            {
                // LRU
                BundleWrapper unUseBundleWrapper = null;
                foreach (var bundleWrapper in _nameToWrapperMap.Values)
                {
                    if (unUseBundleWrapper == null || unUseBundleWrapper.LastAccessTime > bundleWrapper.LastAccessTime && !bundleWrapper.IsActive)
                    {
                        unUseBundleWrapper = bundleWrapper;
                    }
                }

                if (unUseBundleWrapper != null)
                {
                    await unUseBundleWrapper.TryUnloadAsync(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(AssetBundleManager)}.{nameof(OnReport)}：{e.Message}");
            }
        }
    }
}
