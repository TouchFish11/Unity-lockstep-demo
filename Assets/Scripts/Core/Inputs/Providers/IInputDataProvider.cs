using System.Threading.Tasks;

namespace Core.Inputs.Providers
{
    /// <summary>
    /// 输入数据提供器接口
    /// </summary>
    public interface IInputDataProvider
    {
        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <returns></returns>
        Task LoadDataAsync();
        
        /// <summary>
        /// 尝试获取数据
        /// </summary>
        /// <param name="defaultJson">默认配置数据</param>
        /// <param name="overrideJson">覆盖配置数据</param>
        /// <returns></returns>
        bool TryGetData(out string defaultJson, out string overrideJson);
    }
}
