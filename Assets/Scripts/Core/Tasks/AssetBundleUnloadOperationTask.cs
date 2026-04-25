using System;
using System.Threading;
using Core.Tasks.Awaiter;
using UnityEngine;

namespace Core.Tasks
{
    /// <summary>
    /// AssetBundle卸载操作的任务封装类
    /// </summary>
    internal class AssetBundleUnloadOperationTask : TaskBase
    {
        protected override void OnRequestCompleted()
        {
            
        }

        /// <summary>
        /// 获取任务执行结果，若有异常则抛出
        /// </summary>
        /// <exception cref="Exception">任务执行过程中捕获的异常</exception>
        public void GetResult()
        {
            // 如果存在异常，抛出异常给上层处理
            if (_exception != null)
            {
                throw _exception;
            }
        }
        
        /// <summary>
        /// 获取任务的异步等待器
        /// </summary>
        /// <returns>AssetBundle卸载操作的等待器对象</returns>
        public AssetBundleUnloadOperationAwaiter GetAwaiter()
        {
            return new AssetBundleUnloadOperationAwaiter(this);
        }
    }
}