using System.Threading.Tasks;

namespace Core.Inputs.Providers
{
    /// <summary>
    /// 输入数据直接提供器
    /// </summary>
    public class InputDataDirectProvider : InputDataProvider
    {
        public InputDataDirectProvider(string defaultJson, string overrideJson, string configKey = null) : base(configKey)
        {
            ConfigJson = defaultJson;
            OverrideJson = overrideJson;
        }

        /// <summary>
        /// 不加载数据，由构造直接传入
        /// </summary>
        /// <returns></returns>
        public override Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }
    }
}
