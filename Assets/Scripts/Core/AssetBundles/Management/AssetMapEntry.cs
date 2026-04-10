using System;
using Newtonsoft.Json;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源映射项
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class AssetMapEntry
    {
        // 资源的加载key
        [JsonProperty] public string key;     
        // 资源所在的AB包名
        [JsonProperty] public string bundleName;     
        // 资源在AB包内的名称
        [JsonProperty] public string assetName;      
    }
}
