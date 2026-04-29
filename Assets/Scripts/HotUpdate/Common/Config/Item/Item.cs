using Core.Pool;

namespace HotUpdate.Common.Config.Item
{
    /// <summary>
    /// 物品对象，可通过缓存池复用
    /// </summary>
    public class Item : IPoolData
    {
        // 运行时数据——实例ID
        public int instanceId;
        
        // 配置数据
        public ItemConfig itemConfig;
        
        // 玩家数据
        public int auxValue;    // 物品辅助数据，数量/强化等级/收藏星级等
        public bool isNew;
        
        void IPoolData.ResetData()
        {
            itemConfig = null;
        }
    }
}
