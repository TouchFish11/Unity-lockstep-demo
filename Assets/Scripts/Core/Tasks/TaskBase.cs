using System;
using System.Collections.Generic;
using System.Threading;
using Core.Pool;
using UnityEngine;

namespace Core.Tasks
{
    /// <summary>
    /// 任务基类
    /// </summary>
    internal abstract class TaskBase : IPoolData
    {
        // 多线程锁
        protected readonly object _lock = new();
        // Unity异步操作对象
        protected AsyncOperation _operation;
        // 取消令牌，用于监听取消请求
        protected CancellationToken _cancellationToken;
        // 取消令牌注册器，用于释放取消监听
        protected CancellationTokenRegistration _cancellationTokenRegistration;
        // Unity上下文
        protected SynchronizationContext _synchronizationContext;
        // 任务完成后的延续回调列表
        protected readonly List<Action> _continuations = new();
        // 任务执行过程中抛出的异常
        protected Exception _exception;
        // 任务是否完成（volatile保证多线程可见性）
        protected volatile bool _isCompleted;
        
        /// <summary>
        /// 任务是否已完成（完成包括成功、失败、取消）
        /// </summary>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="token"></param>
        public void Init(AsyncOperation operation,  CancellationToken token = default)
        {
            _operation = operation;
            // 注册原生请求完成的回调
            _operation.completed += RequestCompleted;
            // 保存Unity上下文
            _synchronizationContext = SynchronizationContext.Current;
            // 设置取消令牌
            _cancellationToken = token;
            // 如果取消令牌可取消，则注册取消回调
            if (_cancellationToken.CanBeCanceled)
            {
                _cancellationTokenRegistration = _cancellationToken.Register(state =>
                {
                    // 若当前上下文为null，不处理，任务创建应该规范在主线程
                    // 若取消调用在多线程，则延续回调应该被放入主线程处理
                    _synchronizationContext.Post(_ => OnCancelCompleted(state), null);
                }, (this, token));
            }
            else
            {
                // 不可取消的令牌，赋值默认注册器
                _cancellationTokenRegistration = default;
            }
        }
        
        /// <summary>
        /// 取消回调
        /// </summary>
        /// <param name="state">装箱元组，包含任务对象和取消令牌</param>
        private void OnCancelCompleted(object state)
        {
            var (task, token) = ((TaskBase, CancellationToken))state;
            // 检查是否已完成，防止重复处理
            if (task._isCompleted)
            {
                return;
            }

            Action[] continuations;
            // 加锁保证并发安全，防止多线程同时处理取消和完成
            lock (_lock)
            {
                // 双重检查，防止并发场景下的重复处理
                if (task._isCompleted)
                {
                    return;
                }
                        
                // 标记取消异常，供后续抛出
                _exception = new OperationCanceledException(token);
                // 标记任务完成
                _isCompleted = true;
                // 获取所有要执行的延迟任务
                continuations = _continuations.ToArray();
                // 移除原生回调，避免内存泄漏
                _operation.completed -= RequestCompleted;
                _continuations.Clear();
            }

            // 如果已设置延续回调，触发回调通知任务完成
            ExecuteContinuation(continuations);
        }
        
        /// <summary>
        /// 设置任务完成后的延续回调
        /// </summary>
        /// <param name="continuation">延续执行的委托</param>
        public void SetContinuation(Action continuation)
        {
            if(continuation == null)
                return;

            Action callBack = null;
            lock (_lock)
            {
                if (_isCompleted)
                {
                    callBack = continuation;
                }
                else
                {
                    _continuations.Add(continuation);
                }
            }
            callBack?.Invoke();
        }

        /// <summary>
        /// AsyncOperation完成回调
        /// </summary>
        /// <param name="operation">AsyncOperation对象</param>
        private void RequestCompleted(AsyncOperation operation)
        {
            // 防止重复调用（任务可能已被取消）
            if (_isCompleted)
            {
                return;
            }

            Action[] continuations;
            lock (_lock)
            {
                // DCL
                if (_isCompleted)
                {
                    return;
                }
                
                try
                {
                    OnRequestCompleted();
                }
                catch(Exception e)
                {
                    _exception = e;
                }
                finally
                {
                    // 移除原生回调，避免内存泄漏
                    _operation.completed -= RequestCompleted;
                    // 释放取消令牌注册器，取消监听
                    _cancellationTokenRegistration.Dispose();
                    // 修改状态
                    _isCompleted = true;
                    // 获取所有要执行的延迟任务
                    continuations = _continuations.ToArray();
                    _continuations.Clear();
                }
            }
            
            // 锁外执行延续
            ExecuteContinuation(continuations);
        }
        
        /// <summary>
        /// AsyncOperation完成时触发，处理各自的结果
        /// </summary>
        protected abstract void OnRequestCompleted();
        
        /// <summary>
        /// 执行全部延续任务
        /// </summary>
        /// <param name="continuations">延续数组</param>
        private void ExecuteContinuation(Action[] continuations)
        {
            foreach (var continuation in continuations)
            {
                continuation?.Invoke();
            }
        }
        
        void IPoolData.ResetData()
        {
            _operation = null;
            _continuations.Clear();
            _exception = null;
            _isCompleted = false;
            _synchronizationContext = null;
            _cancellationTokenRegistration = default;
            _cancellationToken = CancellationToken.None;
            OnResetData();
        }

        /// <summary>
        /// 被回收到对象池时调用，执行清理
        /// </summary>
        protected virtual void OnResetData()
        {
            
        }
    }
}
