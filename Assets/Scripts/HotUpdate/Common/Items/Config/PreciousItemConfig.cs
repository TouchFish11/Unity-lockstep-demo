using System;

namespace HotUpdate.Common.Items.Config
{
    /// <summary>
    /// 贵重物品配置
    /// </summary>
    [Serializable]
    public class PreciousItemConfig : ItemConfig
    {
        // 物品星级
        public int starLevel;
    }
}
