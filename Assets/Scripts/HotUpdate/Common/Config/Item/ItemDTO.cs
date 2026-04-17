using System;
using Core.Pool;
using UnityEngine;

namespace HotUpdate.Common.Config.Item
{
    /// <summary>
    /// 物品DTO，可通过缓存池复用
    /// </summary>
    public class ItemDTO : IPoolData
    {
        // 配置数据
        public int itemId;
        public string iconPath;
        public EItemQuality qualityType;
        public EItemType itemType;
        // 运行时数据
        public int itemNumOrLv;
        // 引擎数据
        [NonSerialized] public Sprite icon;
        [NonSerialized] public Color qualityBk;
        
        void IPoolData.ResetData()
        {
            icon = null;
        }
    }
}
