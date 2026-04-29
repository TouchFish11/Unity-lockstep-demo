using HotUpdate.Common.Data.Inventory;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 物品类型解析器
    /// </summary>
    public static class ItemResolver
    {
        /// <summary>
        /// 解析不同物品数据类型的数值含义
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static int ResolveAux(ItemData itemData)
        {
            return itemData switch
            {
                HolyRelicData holyRelicData => holyRelicData.level,
                _ => itemData.itemNum
            };
        }
    }
}
