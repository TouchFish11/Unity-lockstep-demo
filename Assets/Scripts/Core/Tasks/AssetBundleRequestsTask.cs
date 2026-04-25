using System;
using System.Collections.Generic;
using System.Threading;
using Core.Tasks.Awaiter;
using UnityEngine;

namespace Core.Tasks
{
    /// <summary>
    /// AB包批量请求资源任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AssetBundleRequestsTask<T> : TaskBase where T : class
    {
        // 加载成功后的资源结果
        private readonly List<T> _result = new();
        
        protected override void OnRequestCompleted()
        {
            var _abr = (AssetBundleRequest)_operation;
            // 成功优先级大于取消
            foreach (var asset in _abr.allAssets)
            {
                _result.Add(asset as T);
            }
        }
        
        /// <summary>
        /// 获取任务执行结果
        /// </summary>
        /// <returns>加载成功的资源对象</returns>
        /// <exception cref="Exception">任务执行过程中抛出的异常（包括取消异常）</exception>
        public IReadOnlyList<T> GetResult()
        {
            // 如果有异常则抛出
            return _exception != null ? throw _exception : _result;
        }
        
        /// <summary>
        /// 获取异步等待器，支持await语法
        /// </summary>
        /// <returns>AssetBundle请求等待器</returns>
        public AssetBundleRequestsAwaiter<T> GetAwaiter()
        {
            return new AssetBundleRequestsAwaiter<T>(this);
        }
        
        protected override void OnResetData()
        {
            _result.Clear();
        }
    }
}
