namespace HotUpdate.Base.Grid
{
    /// <summary>
    /// 格子对象基类
    /// 格子类必须继承该接口
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IGridBase<T> : IGridInteractive<T>
    {
        /// <summary>
        /// 初始化格子数据
        /// </summary>
        /// <param name="data">数据类型</param>
        void InitGrid(T data);
    }
}
