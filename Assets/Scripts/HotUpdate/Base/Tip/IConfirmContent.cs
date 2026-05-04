namespace HotUpdate.Base.Tip
{
    /// <summary>
    /// 提示内容接口
    /// </summary>
    public interface IConfirmContent
    {
        /// <summary>
        /// 绘制提示内容
        /// </summary>
        /// <param name="drawDatas">绘制数据</param>
        void DrawContent(object drawDatas);
        
        void ClearContent();
    }
}
