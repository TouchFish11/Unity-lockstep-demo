using System;
using System.Threading;
using Core.Tasks.Awaiter;
using UnityEngine;

namespace Core.Tasks
{
    /// <summary>
    /// AssetBundle创建请求的异步任务封装类
    /// 用于管理AssetBundleCreateRequest的生命周期、取消逻辑和回调通知
    /// </summary>
    internal class AssetBundleCreateRequestTask : TaskBase
    {
        // 任务执行结果：成功时存储加载的AssetBundle
        private AssetBundle _result;
        
        protected override void OnRequestCompleted()
        {
            var _abcr = (AssetBundleCreateRequest)_operation;
            _result = _abcr.assetBundle;
        }
        
        /// <summary>
        /// 获取任务执行结果
        /// </summary>
        /// <returns>成功时返回加载的AssetBundle</returns>
        /// <exception cref="Exception">任务取消/失败时抛出对应的异常</exception>
        public AssetBundle GetResult()
        {
            // 如果有异常（取消/加载失败），抛出异常；否则返回结果
            return _exception != null ? throw _exception : _result;
        }
        
        /// <summary>
        /// 获取任务的异步等待器，支持await语法
        /// </summary>
        /// <returns>AssetBundle创建请求的等待器</returns>
        public AssetBundleCreateRequestAwaiter GetAwaiter()
        {
            return new AssetBundleCreateRequestAwaiter(this);
        }
    }
}