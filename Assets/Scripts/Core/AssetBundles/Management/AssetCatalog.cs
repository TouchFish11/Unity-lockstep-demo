using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 资源目录
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class AssetCatalog
    {
        [JsonProperty] private Dictionary<string, AssetMapEntry> nameToAssetMap = new();

        /// <summary>
        /// 获取资源条目
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AssetMapEntry GetEntry(string assetName)
        {
            return nameToAssetMap.GetValueOrDefault(assetName);
        }

        public AssetMapEntry[] GetEntries(params string[] assetNames)
        {
            List<AssetMapEntry> list = new();
            foreach (var assetName in assetNames)
            {
                if (nameToAssetMap.TryGetValue(assetName, out var entry))
                {
                    list.Add(entry);
                }
            }
            return list.ToArray();
        }
    }
}
