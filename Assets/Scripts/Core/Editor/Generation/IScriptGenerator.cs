namespace Core.Editor.Generation
{
    /// <summary>
    /// 脚本器生成接口
    /// </summary>
    public interface IScriptGenerator
    {
        string FilePath { get; }
        
        /// <summary>
        /// 生成脚本
        /// </summary>
        void GenerateScript();
    }
}
