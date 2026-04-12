using System;
using System.Collections.Generic;
using Core.AssetBundles.Update.Collection;
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
        // 资源Key到资源映射条目的映射
        [JsonProperty] private Dictionary<string, AssetMapEntry> assetMap = new();
        // AB包集合
        [JsonProperty] private ABPackageCollection abPackageCollection = new();
        
        /// <summary>
        /// AB包清单集合
        /// </summary>
        public ABPackageCollection ABPackageCollection => abPackageCollection;
        
        /// <summary>
        /// 资源的所有Key
        /// </summary>
        public Dictionary<string, AssetMapEntry>.KeyCollection AssetKeys => assetMap.Keys;
        
        /// <summary>
        /// 所有资源的Values
        /// </summary>
        public Dictionary<string, AssetMapEntry>.ValueCollection Assets => assetMap.Values;

        public AssetMapEntry this[string key]
        {
            get => assetMap[key];
            set => assetMap[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return assetMap.ContainsKey(key);
        }

        public void AddEntry(string key, AssetMapEntry entry)
        {
            assetMap.Add(key, entry);
        }

        public bool RemoveEntry(string key)
        {
            return assetMap.Remove(key);
        }
        
        /// <summary>
        /// 获取资源条目
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AssetMapEntry GetEntry(string key)
        {
            return assetMap.GetValueOrDefault(key);
        }

        public AssetMapEntry[] GetEntries(params string[] keys)
        {
            List<AssetMapEntry> list = new();
            foreach (var assetName in keys)
            {
                if (assetMap.TryGetValue(assetName, out var entry))
                {
                    list.Add(entry);
                }
            }
            return list.ToArray();
        }
    }
}
