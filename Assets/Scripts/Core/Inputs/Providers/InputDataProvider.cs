using System.Threading.Tasks;

namespace Core.Inputs.Providers
{
    /// <summary>
    /// 输入数据提供器基类
    /// </summary>
    public abstract class InputDataProvider : IInputDataProvider
    {
        /// <summary>
        /// 配置资源键
        /// </summary>
        protected string ConfigKey { get; }
        
        /// <summary>
        /// 配置Json
        /// </summary>
        protected string ConfigJson { get; set; }
        
        /// <summary>
        /// 覆盖Json
        /// </summary>
        protected string OverrideJson { get; set; }
        
        protected InputDataProvider(string configKey)
        {
            ConfigKey = configKey;
        }

        public abstract Task LoadDataAsync();

        public bool TryGetData(out string defaultJson, out string overrideJson)
        {
            if (string.IsNullOrEmpty(ConfigJson))
            {
                defaultJson = string.Empty;
                overrideJson = string.Empty;
                return false;
            }
            
            defaultJson = ConfigJson;
            overrideJson = OverrideJson;
            return true;
        }
    }
}
