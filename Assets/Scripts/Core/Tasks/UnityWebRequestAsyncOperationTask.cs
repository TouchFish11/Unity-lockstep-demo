using System;
using System.Threading;
using Core.Tasks.Awaiter;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace Core.Tasks
{
    /// <summary>
    /// UnityWebRequest异步操作任务
    /// </summary>
    internal class UnityWebRequestAsyncOperationTask : TaskBase
    {
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
        
        protected override void OnRequestCompleted()
        {
            
        }

        /// <summary>
        /// 获取任务的异步等待器
        /// </summary>
        /// <returns>AssetBundle卸载操作的等待器对象</returns>
        public UnityWebRequestAsyncOperationAwaiter GetAwaiter()
        {
            return new UnityWebRequestAsyncOperationAwaiter(this);
        }
    }
}
