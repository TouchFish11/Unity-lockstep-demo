using System;
using System.Threading;
using Core.Tasks.Awaiter;
using UnityEngine;

namespace Core.Tasks
{
    /// <summary>
    /// AssetBundle单个资源请求任务类
    /// </summary>
    /// <typeparam name="T">要加载的资源类型</typeparam>
    internal class AssetBundleRequestTask<T> : TaskBase where T : class
    {
        // 加载成功后的资源结果
        private T _result;
        
        protected override void OnRequestCompleted()
        {
            var _abr = (AssetBundleRequest)_operation;
            _result = _abr.asset as T;
        }
        
        /// <summary>
        /// 获取任务执行结果
        /// </summary>
        /// <returns>加载成功的资源对象</returns>
        /// <exception cref="Exception">任务执行过程中抛出的异常（包括取消异常）</exception>
        public T GetResult()
        {
            // 如果有异常则抛出，否则返回资源结果
            return _exception != null ? throw _exception : _result;
        }
        
        /// <summary>
        /// 获取异步等待器，支持await语法
        /// </summary>
        /// <returns>AssetBundle请求等待器</returns>
        public AssetBundleRequestAwaiter<T> GetAwaiter()
        {
            return new AssetBundleRequestAwaiter<T>(this);
        }
    }
}