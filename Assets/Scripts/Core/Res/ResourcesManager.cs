using System.Collections;
using System.Collections.Generic;
using Core.Mono;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Res
{
    /// <summary>
    /// Resources
    /// </summary>
    public class ResourcesManager : IResourcesManager
    {
        private readonly IMonoAdapter _monoAdapter;
        
        // 资源名称到资源信息的映射
        private readonly Dictionary<string, BaseResourcesInfo> _nameToResInfoMap = new();

        private ResourcesManager(IMonoAdapter monoAdapter)
        {
            _monoAdapter = monoAdapter;
        }
        
        public T Load<T>(string resPath) where T : Object
        {
            // 自动生成存储名
            var cacheName = $"{resPath}_{typeof(T).Name}";
            ResourcesInfo<T> info = null;
            if (_nameToResInfoMap.TryGetValue(cacheName, out var value))
            {
                info = value as ResourcesInfo<T>;
                if (info != null && !info.Asset)
                {
                    _monoAdapter.StopCoroutine(info.ResCoroutine);
                    // 清空协程
                    info.ResCoroutine = null;
                    // 同步加载，记录资源
                    info.Asset = Resources.Load<T>(resPath);
                    // 执行回调
                    info.Invoke();
                }

                return info.Asset;
            }

            info = new ResourcesInfo<T>(null);
            // 存储到字典
            _nameToResInfoMap.Add(cacheName, info);
            // 同步加载，记录资源
            info.Asset = Resources.Load<T>(resPath);
            return info.Asset;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resName">资源路径</param>
        /// <param name="callBack">回调函数</param>
        public void LoadAsync<T>(string resName, UnityAction<T> callBack) where T : Object
        {
            // 自动生成存储名
            string cacheName = $"{resName}_{typeof(T).Name}";

            ResourcesInfo<T> info;
            if (_nameToResInfoMap.ContainsKey(cacheName))
            {
                info = _nameToResInfoMap[cacheName] as ResourcesInfo<T>;
                // 增加引用计数
                ++info.RefCount;
                // 等待异步加载资源
                if (info.Asset == null)
                    info.ResCallBack += callBack;
                else
                    callBack?.Invoke(info.Asset);
                return;
            }

            info = new ResourcesInfo<T>(callBack);
            _nameToResInfoMap.Add(cacheName, info);

            // 通过Mono适配器开启协程
            info.ResCoroutine = _monoAdapter.StartCoroutine(LoadAsync_Cor());

            IEnumerator LoadAsync_Cor()
            {
                // 异步加载资源
                ResourceRequest req = Resources.LoadAsync<T>(resName);
                yield return req;
                ResourcesInfo<T> info = _nameToResInfoMap[cacheName] as ResourcesInfo<T>;
                // 如果没被删除，记录资源并执行回调
                if (!info.IsDelete)
                {
                    // 记录资源
                    info.Asset = req.asset as T;
                    // 执行回调
                    info.Invoke();
                }
                // 否则不记录资源，卸载资源并从字典移除
                else
                    UnloadAsset<T>(resName);
            }
        }

        /// <summary>
        /// 卸载指定资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resName">资源名</param>
        public void UnloadAsset<T>(string resName) where T : Object
        {
            // 自动生成存储名
            string cacheName = $"{resName}_{typeof(T).Name}";
            ResourcesInfo<T> info;

            // 如果字典中存在该资源，说明资源正在异步加载或已加载
            if (_nameToResInfoMap.ContainsKey(cacheName))
            {
                info = _nameToResInfoMap[cacheName] as ResourcesInfo<T>;
                if(!info.IsDelete)
                    // 如果不是已删除的资源，才减少引用计数
                    --info.RefCount;
                // 引用计数为0时，标记资源为待删除资源
                if(info.RefCount == 0 && !info.IsDelete)
                    info.IsDelete = true;
                // 资源已加载完成
                if (info.Asset != null && info.IsDelete)
                {
                    if (info.Asset is not GameObject)
                        // 卸载资源
                        Resources.UnloadAsset(info.Asset);

                    // 清空资源引用
                    info.Asset = null;
                    // 从字典移除
                    _nameToResInfoMap.Remove(cacheName);
                }
                // 如果资源正在异步加载，会在回调里处理
            }
        }

        /// <summary>
        /// 卸载所有未使用的资源
        /// </summary>
        /// <param name="callBack">卸载完成回调</param>
        public void UnloadUnusedAssets(UnityAction callBack = null)
        {
            _monoAdapter.StartCoroutine(UnLoadUnusedAssets_Cor(callBack));
            return;

            static IEnumerator UnLoadUnusedAssets_Cor(UnityAction callBack = null)
            {
                AsyncOperation ao = Resources.UnloadUnusedAssets();
                yield return ao;
                callBack?.Invoke();
            }
        }

        /// <summary>
        /// 清空所有资源
        /// </summary>
        public void Clear()
        {
            _nameToResInfoMap.Clear();
            UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
