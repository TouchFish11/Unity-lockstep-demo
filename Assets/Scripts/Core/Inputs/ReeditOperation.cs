using System;
using Core.DI;
using Core.Pool;

namespace Core.Inputs
{
    /// <summary>
    /// 重编辑操作操作
    /// </summary>
    public class ReeditOperation : IPoolData
    {
        [Inject] private IPoolManager _poolManager;
        
        private Action<ReeditOperation> _onApply;
        private Action<ReeditOperation> _onCanCel;
        internal event Action<bool> onConflict;
        
        /// <summary>
        /// 冲突类型
        /// </summary>
        public EKeyConflict ConflictType { get; internal set; }
        
        /// <summary>
        /// 交换
        /// </summary>
        public void Exchange()
        {
            onConflict?.Invoke(true);
            onConflict = null;
        }
        
        /// <summary>
        /// 不交换
        /// </summary>
        public void NonExchange()
        {
            onConflict?.Invoke(false);
            onConflict = null;
        }
        
        /// <summary>
        /// 改键应用回调
        /// </summary>
        /// <param name="onApply"></param>
        /// <returns></returns>
        public ReeditOperation OnApplyBinding(Action<ReeditOperation> onApply)
        {
            _onApply = onApply;
            return this;
        }

        /// <summary>
        /// 改键取消回调
        /// </summary>
        /// <param name="onCancel"></param>
        /// <returns></returns>
        public ReeditOperation OnCancel(Action<ReeditOperation> onCancel)
        {
            _onCanCel = onCancel;
            return this;
        }
        
        internal void Cancel()
        {
            _onCanCel?.Invoke(this);
            _onCanCel = null;
        }

        internal void Apply()
        {
            _onApply?.Invoke(this);
            _onApply = null;
        }
        
        void IPoolData.ResetData()
        {
            _onApply = null;
            _onCanCel = null;
            onConflict = null;
        }

        /// <summary>
        /// 回收对象到对象池中
        /// </summary>
        public void Dispose()
        {
            if(_poolManager == null)
                return;
            
            _poolManager.PushData(this);
            _poolManager = null;
        }
    }
}
