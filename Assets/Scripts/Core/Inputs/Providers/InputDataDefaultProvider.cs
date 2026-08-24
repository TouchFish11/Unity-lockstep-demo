using System.IO;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.Utility;
using UnityEngine;

namespace Core.Inputs.Providers
{
    /// <summary>
    /// 输入数据默认提供器
    /// </summary>
    public class InputDataDefaultProvider : InputDataProvider
    {
        /// <summary>
        /// 本地覆盖文件路径
        /// </summary>
        private static string LocalPath => PathUtility.GetUserDataLocalSavePath(FileSources.InputActionLocalFileName);
        
        public InputDataDefaultProvider(string configKey) : base(configKey)
        {
            
        }

        /// <summary>
        /// 提供器缓存本地读取路径<see cref="LocalPath"/>，若不存在<see cref="InputDataProvider.OverrideJson"/>则为<see cref="string.Empty"/>
        /// </summary>
        public override async Task LoadDataAsync()
        {
            var textAsset = await GameAsset.LoadAssetAsync<TextAsset>(ConfigKey);
            ConfigJson = textAsset.Asset.text;
            if (File.Exists(LocalPath))
            {
                OverrideJson = await File.ReadAllTextAsync(LocalPath);
            }
            else
            {
                OverrideJson = string.Empty;
            }
        }
    }
}
