using System.IO;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using UnityEngine;

namespace Core.Inputs.Providers
{
    /// <summary>
    /// 输入数据路径提供器
    /// </summary>
    public class InputDataPathProvider : InputDataProvider
    {
        // 读取路径
        private readonly string _overridePath;
        
        public InputDataPathProvider(string configKey, string overridePath) : base(configKey)
        {
            _overridePath = overridePath;
        }

        /// <summary>
        /// 根据<see cref="_overridePath"/>读取本地数据，若不存在则返回<see cref="string.Empty"/>
        /// </summary>
        public override async Task LoadDataAsync()
        {
            var textAsset = await GameAsset.LoadAssetAsync<TextAsset>(ConfigKey);
            ConfigJson = textAsset.Asset.text;
            if (File.Exists(_overridePath))
            {
                OverrideJson = await File.ReadAllTextAsync(_overridePath);
            }
            else
            {
                OverrideJson = string.Empty;
            }
        }
    }
}
