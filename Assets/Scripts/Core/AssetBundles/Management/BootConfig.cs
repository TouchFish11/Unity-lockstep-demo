using System;

namespace Core.AssetBundles.Management
{
    [Serializable]
    public class BootConfig
    {
        // 热更 DLL 所在的 AB 包名（带 .assetBundle 后缀）
        public string hotfixDllBundleName;
        // 版本号，用于调试
        public string version;
    }
}