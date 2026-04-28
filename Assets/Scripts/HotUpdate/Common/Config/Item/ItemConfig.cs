using System;

namespace HotUpdate.Common.Config.Item
{
    /// <summary>
    /// 物品配置
    /// </summary>
    [Serializable]
    public class ItemConfig
    {
        // 物品ID
        public int itemId;
        // 物品名称
        public string name;
        // 物品描述
        public string description;
        // 物品图标
        public string icon;
        // 是否可堆叠
        public bool isPile;
        // 物品品质
        public EItemQuality itemQuality;
        // 物品类型
        public EItemType itemType;
        
        //...
    }
}
