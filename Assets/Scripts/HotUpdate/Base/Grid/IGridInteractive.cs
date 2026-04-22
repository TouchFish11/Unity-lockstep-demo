using System;

namespace HotUpdate.Base.Grid
{
    /// <summary>
    /// 格子交互接口，需要处理点击等事件的格子需要实现接口，外部通过就饿看注册事件回调
    /// </summary>
    public interface IGridInteractive<out T>
    {
        event Action<T> OnClick;
        
        void TriggerClick();
    }
}
