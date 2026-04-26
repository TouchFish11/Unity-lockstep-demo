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
using Task = System.Threading.Tasks.Task;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 包包装器
    /// </summary>
    internal class BundleWrapper
    {
        private class LoadState<T> where T : class
        {
            public TaskHandle<T> taskHandle;
            public bool retained;
        }
        
        // AB包管理器
        private readonly IAssetBundleManager _assetBundleManager;
        // AB包加载任务句柄
        private TaskHandle<AssetBundle> _assetBundleCreateRequestTaskHandle;
        
        // AB包卸载任务
        private TaskHandle _assetBundleUnloadTaskHandle;
        // 物理文件到加载任务的映射缓存
        private readonly Dictionary<string, object> _assetTasks =  new();
        // 批量加载资源任务缓存
        private readonly Dictionary<Type, object> _assetBundleRequestsTasks = new();
        // LFU滑动窗口
        private readonly LFUSlidingWindow _window;
        
        /// <summary>
        /// AssetBundle对象
        /// </summary>
        private AssetBundle AssetBundle { get; set; }
        
        /// <summary>
        /// 包名称
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// 包加载路径
        /// </summary>
        public string LoadPath { get; }
        
        /// <summary>
        /// 包引用数，当前存活的资源引用数
        /// </summary>
        public uint RefCount { get; private set; }
        
        /// <summary>
        /// 上次访问的时间
        /// </summary>
        public double LastAccessTime { get; private set; }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 获取当前包 LFU 热度值
        /// </summary>
        public int AccessCount => _window.GetCurrentHotness();
        
        /// <summary>
        /// 在访问资源时触发回调
        /// </summary>
        public Action<BundleWrapper> OnAccessAsset;
        
        /// <summary>
        /// AB包是否为null
        /// </summary>
        public bool IsNull => !AssetBundle;
        
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

        /// <summary>
        /// 记录访问次数（热度）
        /// </summary>
        public void RecordAccess()
        {
            LastAccessTime =  TimeUtil.RealtimeSinceStartupAsDouble;
            _window.RecordAccess();
            OnAccessAsset?.Invoke(this);
        }
        
        /// <summary>
        /// 从文件加载AssetBundle
        /// </summary>
        /// <returns></returns>
        public void LoadFromFile()
        {
            try
            {
                // 已加载完成，直接返回，避免重复加载
                if (AssetBundle)
                {
                    IsActive = true;
                    return;
                }
                
                // 加载AB包
                AssetBundle = AssetBundle.LoadFromFile(LoadPath);
                IsActive = true;
                Logger.Log($"[BundleWrapper]: '{BundleName}' assetBundle is load");
            }
            catch (Exception e)
            {
                Logger.LogError($"[BundleWrapper]: '{BundleName}' assetBundle Load fail, {e.Message}");
            }
        }
        
        /// <summary>
        /// 从文件异步加载AssetBundle
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadFromFileAsync(CancellationToken token = default)
        {
            // 已加载完成，直接返回，避免重复加载
            if (AssetBundle)
            {
                IsActive = true;
                return;
            }

            // 正在异步加载
            if (_assetBundleCreateRequestTaskHandle.IsValid)
            {
                try
                {
                    // 等待同一个任务加载
                    AssetBundle ??= await _assetBundleCreateRequestTaskHandle.Task;
                    IsActive = true;
                    return;
                }
                catch (Exception e)
                {
                    Logger.LogError($"[BundleWrapper]: '{BundleName}' assetBundle Load fail, {e.Message}");
                }
                finally
                {
                    _assetBundleCreateRequestTaskHandle.Dispose();
                }
            }
            else
            {
                try
                {
                    // 异步加载AB包
                    _assetBundleCreateRequestTaskHandle = AssetBundle.LoadFromFileAsync(LoadPath).ToTask(token);
                    AssetBundle ??= await _assetBundleCreateRequestTaskHandle.Task;
                    IsActive = true;
                    Logger.Log($"[BundleWrapper]: '{BundleName}' assetBundle is load");
                }
                catch (Exception e)
                {
                    Logger.LogError($"[BundleWrapper]: '{BundleName}' assetBundle Load fail, {e.Message}");
                }
                finally
                {
                    _assetBundleCreateRequestTaskHandle.Dispose();
                }
            }
        }
        
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetKey"></param>
        /// <param name="assetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AssetWrapper LoadAsset<T>(string assetKey, string assetName) where T : Object
        {
            var asset = AssetBundle.LoadAsset<T>(assetName);
            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { asset, assetKey, this });
        }
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetKey"></param>
        /// <param name="assetName"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<AssetWrapper> LoadAssetAsync<T>(string assetKey, string assetName, CancellationToken token = default) where T : class
        {
            if (!_assetTasks.TryGetValue(assetKey, out var state))
            {
                // 创建新加载状态
                var newState = new LoadState<T> { taskHandle = AssetBundle.LoadAssetAsync<T>(assetName).ToTask<T>(token) };
                try
                {
                    // 先缓存加载状态
                    _assetTasks.Add(assetKey, newState);
                    // 等待任务结果
                    var asset = await newState.taskHandle.Task;
                    // 没有增加过计数，才去增加
                    if (!newState.retained)
                    {
                        Retain();
                        newState.retained = true;
                    }
                    return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { asset, assetKey, this });
                }
                catch(Exception e)
                {
                    Logger.LogError($"[BundleWrapper]: '{BundleName}/{assetKey}' asset Load fail, {e.Message}");
                }
                finally
                {
                    newState.taskHandle.Dispose();
                    _assetTasks.Remove(assetKey);
                }
            }
            else
            {
                var cacheState = (LoadState<T>)state;
                try
                {
                    // 等待同一加载任务结果
                    var asset = await cacheState.taskHandle.Task;
                    // 没有增加过计数，才去增加
                    if (!cacheState.retained)
                    {
                        Retain();
                        cacheState.retained = true;
                    }

                    return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { asset, assetKey, this });
                }
                catch (Exception e)
                {
                    Logger.LogError($"[BundleWrapper]: '{BundleName}/{assetKey}' asset Load fail, {e.Message}");
                }
                finally
                {
                    cacheState.taskHandle.Dispose();
                    _assetTasks.Remove(assetKey);
                }
            }

            return DIContainer.Create<AssetWrapper>(parameterValues: new object[] { null, assetKey, this });
        }
        
        /// <summary>
        /// 异步加载所有资源
        /// </summary>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<AssetWrapper[]> LoadAllAssetAsync<T>(CancellationToken token = default) where T : Object
        {
            if (!_assetBundleRequestsTasks.TryGetValue(typeof(T), out var obj))
            {
                var task = LoadAllAssetAsyncInternal<T>(token);
                _assetBundleRequestsTasks.Add(typeof(T), task);
                return await task;
            }
            
            return await (Task<AssetWrapper[]>)obj;
        }

        /// 异步加载所有资源（内部）
        private async Task<AssetWrapper[]> LoadAllAssetAsyncInternal<T>(CancellationToken token = default) where T : Object
        {
            var handle = AssetBundle.LoadAllAssetsAsync<T>().ToTasks<T>(token);
            try
            {
                var readOnlyAssets = await handle.Task;
                var assetWrappers = new List<AssetWrapper>(readOnlyAssets.Count);
                foreach (var asset in readOnlyAssets)
                {
                    assetWrappers.Add(DIContainer.Create<AssetWrapper>(parameterValues: new object[] { asset, asset.name, this }));
                    Retain();
                }

                return assetWrappers.ToArray();
            }
            catch (Exception e)
            {
                Logger.LogError($"[BundleWrapper]: '{BundleName}' assetBundle Load all asset fail, {e.Message}");
                return Array.Empty<AssetWrapper>();
            }
            finally
            {
                handle.Dispose();
                // 无论成败都移除，允许后续重新加载
                _assetBundleRequestsTasks.Remove(typeof(T));
            }
        }
        
        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Retain()
        {
            ++RefCount;
            Logger.Log($"[BundleWrapper]: '{BundleName}' assetBundle is referenced, refCount updated to {RefCount}");
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
            
            Logger.Log($"[BundleWrapper]: '{BundleName}' assetBundle is released, refCount updated to {RefCount}");
        }

        /// <summary>
        /// 尝试异步卸载AB包
        /// </summary>
        /// <param name="unloadAllLoadedObjects"></param>
        public async Task TryUnloadAsync(bool unloadAllLoadedObjects)
        {
            // 卸载完成返回
            if (!AssetBundle)
            {
                return;
            }
            
            // 正在异步卸载，等待卸载
            if (_assetBundleUnloadTaskHandle.IsValid)
            {
                try
                {
                    await _assetBundleUnloadTaskHandle.Task;
                }
                catch (Exception e)
                {
                    Logger.Log($"[BundleWrapper]: '{BundleName}' is unload fail, final refCount is {RefCount}, {e.Message}");
                }
                finally
                {
                    _assetBundleUnloadTaskHandle.Dispose();
                }
            }
            else
            {
                try
                {
                    // 异步卸载AB包
                    _assetBundleUnloadTaskHandle = AssetBundle.UnloadAsync(unloadAllLoadedObjects).ToTask();
                    await _assetBundleUnloadTaskHandle.Task;
                    // 卸载完成后置空
                    AssetBundle = null;
                    Logger.Log($"[BundleWrapper]: '{BundleName}' is unload, final refCount is {RefCount}");
                }
                catch (Exception e)
                {
                    Logger.Log($"[BundleWrapper]: '{BundleName}' is unload fail, final refCount is {RefCount}, {e.Message}");
                }
                finally
                {
                    _assetBundleUnloadTaskHandle.Dispose();
                }
            }
        }
    }
}
