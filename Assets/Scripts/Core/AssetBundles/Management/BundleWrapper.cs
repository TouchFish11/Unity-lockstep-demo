using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.DI;
using Core.Tasks;
using Core.Tasks.Extensions;
using Core.Utility;
using UnityEngine;
using Logger = Core.Log.Logger;
using Object = UnityEngine.Object;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 包包装器
    /// </summary>
    internal class BundleWrapper
    {
        // AB包管理器
        private readonly IAssetBundleManager _assetBundleManager;
        // AB包加载任务
        private AssetBundleCreateRequestTask _assetBundleCreateRequestTask;
        // AB包卸载任务
        private AssetBundleUnloadOperationTask _assetBundleUnloadTask;
        // LFU滑动窗口
        private readonly LFUSlidingWindow _window;
        
        /// <summary>
        /// AssetBundle对象
        /// </summary>
        private AssetBundle AssetBundle { get; set; }
        
        /// <summary>
        /// 包名称
        /// </summary>
        internal string BundleName { get; }

        /// <summary>
        /// 包加载路径
        /// </summary>
        internal string LoadPath { get; }
        
        /// <summary>
        /// 包引用数，当前存活的资源引用数
        /// </summary>
        internal uint RefCount { get; private set; }
        
        /// <summary>
        /// 上次访问的时间
        /// </summary>
        internal double LastAccessTime { get; private set; }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        internal bool IsActive { get; set; }

        /// <summary>
        /// 获取当前包 LFU 热度值
        /// </summary>
        internal int AccessCount => _window.GetCurrentHotness();
        
        /// <summary>
        /// 在访问资源时触发回调
        /// </summary>
        internal Action<BundleWrapper> OnAccessAsset;
        
        /// <summary>
        /// 包装载器
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="path"></param>
        /// <param name="assetBundleManager"></param>
        /// <param name="window"></param>
        public BundleWrapper(string abName, string path, IAssetBundleManager assetBundleManager, LFUSlidingWindow window)
        {
            BundleName = abName;
            LoadPath = path;
            _assetBundleManager = assetBundleManager;
            _window = window;
        }

        public void RecordAccess()
        {
            LastAccessTime =  TimeUtil.RealtimeSinceStartupAsDouble;
            _window.RecordAccess();
            OnAccessAsset?.Invoke(this);
        }

        /// <summary>
        /// 从文件异步加载AssetBundle
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadFromFileAsync(CancellationToken token = default)
        {
            try
            {
                // 已加载完成，直接返回，避免重复加载
                if (AssetBundle)
                {
                    RefCount += 1;
                    IsActive = true;
                    Logger.Log($"[AssetBundle]:{BundleName} is referenced, and the reference count is updated to {RefCount}");
                    return;
                }
        
                // 正在异步加载，等待加载完成，引用计数增加，避免并发问题重复加载
                if (_assetBundleCreateRequestTask != null)
                {
                    AssetBundle ??= await _assetBundleCreateRequestTask;
                    RefCount += 1;
                    IsActive = true;
                    return;
                }
        
                // 异步加载AB包
                _assetBundleCreateRequestTask = AssetBundle.LoadFromFileAsync(LoadPath).ToTask(token);
                AssetBundle = await _assetBundleCreateRequestTask;
                _assetBundleCreateRequestTask = null;
                RefCount += 1;
                IsActive = true;
                Logger.Log($"[AssetBundle]:{BundleName} is referenced, and the reference count is updated to {RefCount}");
            }
            catch (Exception e)
            {
                Logger.LogError($"[AssetBundle]:{BundleName} Load fail,{e.Message}");
                _assetBundleCreateRequestTask = null;
            }
        }
        
        public async Task<AssetWrapper> LoadAssetAsync<T>(string assetName, CancellationToken token = default) where T : class
        {
            var asset = await AssetBundle.LoadAssetAsync<T>(assetName).ToTask<T>(token);
            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { asset, this });
        }

        public async Task<AssetWrapper> LoadAssetsAsync<T>(CancellationToken token = default, params string[] assetNames) where T : class
        {
            IList<T> list = new List<T>();
            foreach (var assetName in assetNames)
            {
                var asset = await AssetBundle.LoadAssetAsync<T>(assetName).ToTask<T>(token);
                list.Add(asset);
            }
            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { list, this });
        }

        public async Task<AssetWrapper> LoadAllAssetAsync<T>(CancellationToken token = default) where T : Object
        {
            IList<T> list = new List<T>();
            await AssetBundle.LoadAllAssetsAsync<T>().ToTask(list, token);
            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { list, this });
        }
        
        /// <summary>
        /// 释放指定AssetBundle，仅减少引用计数
        /// </summary>
        /// <returns></returns>
        public void Release()
        {
            if (RefCount > 0)
            {
                RefCount -= 1;
            }

            if (RefCount == 0)
            {
                IsActive = false;
                _assetBundleManager.ReleaseDependencies(BundleName);
            }
            
            Logger.Log($"[AssetBundle]:{BundleName} is released, and the reference count is updated to {RefCount}");
        }

        /// <summary>
        /// 尝试异步卸载AB包
        /// </summary>
        /// <param name="unloadAllLoadedObjects"></param>
        public async Task TryUnloadAsync(bool unloadAllLoadedObjects)
        {
            // 正在异步卸载，等待卸载
            if (_assetBundleUnloadTask != null)
            {
                await _assetBundleUnloadTask;
            }

            // 卸载完成返回
            if (!AssetBundle)
            {
                return;
            }
            
            // 异步卸载AB包
            _assetBundleUnloadTask = AssetBundle.UnloadAsync(unloadAllLoadedObjects).ToTask();
            await _assetBundleUnloadTask;
            // 卸载完成后置空
            AssetBundle = null;
            _assetBundleUnloadTask = null;
            Logger.Log($"[AssetBundle]:{BundleName} is unload, and the final reference count is {RefCount}");
        }
    }
}
