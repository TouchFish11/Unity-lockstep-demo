using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Tasks;
using Core.Tasks.Extensions;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 包包装器
    /// </summary>
    internal class BundleWrapper
    {
        /// <summary>
        /// AssetBundle对象
        /// </summary>
        internal AssetBundle AssetBundle { get; private set; }
        
        /// <summary>
        /// 包名称
        /// </summary>
        internal string BundleName { get; }

        /// <summary>
        /// 包加载路径
        /// </summary>
        internal string LoadPath { get; }
        
        /// <summary>
        /// 包引用数
        /// </summary>
        internal uint RefCount { get; private set; }
        
        /// <summary>
        /// 上次使用的时间
        /// </summary>
        internal DateTime LastUseTime { get; private set; }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        internal bool IsActive { get; set; }
        
        // AB包管理器
        private readonly IAssetBundleManager _assetBundleManager;
        // AB包加载任务
        private AssetBundleCreateRequestTask _assetBundleCreateRequestTask;
        // AB包卸载任务
        private AssetBundleUnloadOperationTask _assetBundleUnloadTask;

        /// <summary>
        /// 包装载器
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="path"></param>
        /// <param name="assetBundleManager"></param>
        public BundleWrapper(string abName, string path, IAssetBundleManager assetBundleManager)
        {
            BundleName = abName;
            LoadPath = path;
            _assetBundleManager = assetBundleManager;
        }

        /// <summary>
        /// 从文件异步加载AssetBundle
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadFromFileAsync(CancellationToken token = default)
        {
            // 正在异步加载，等待加载完成，避免多线程并发问题
            if (_assetBundleCreateRequestTask != null)
            {
                await _assetBundleCreateRequestTask;
            }
            
            // 已加载完成，直接返回，避免重复加载
            if (AssetBundle)
            {
                RefCount += 1;
                LastUseTime = DateTime.Now;
                IsActive = true;
                Logger.Log($"{BundleName}包被引用，引用计数更新为：{RefCount}");
                return;
            }
            
            // 异步加载AB包
            _assetBundleCreateRequestTask = AssetBundle.LoadFromFileAsync(LoadPath).ToTask(token);
            AssetBundle = await _assetBundleCreateRequestTask;
            RefCount += 1;
            LastUseTime = DateTime.Now;
            _assetBundleCreateRequestTask = null;
            IsActive = true;
            Logger.Log($"{BundleName}包被引用，引用计数更新为：{RefCount}");
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
            
            Logger.Log($"{BundleName}包，引用计数减少，更新为：{RefCount}");
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
            Logger.Log($"{BundleName}包已被卸载，引用计数为：{RefCount}");
        }
    }
}
