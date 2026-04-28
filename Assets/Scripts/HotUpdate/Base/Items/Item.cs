using HotUpdate.Common.Config.Item;

namespace HotUpdate.Base.Items
{
    /// <summary>
    /// 玩家物品对象实例
    /// </summary>
    public class Item
    {
        public int InstanceId { get; }
        
        public ItemConfig ItemConfig { get; }

        public Item(int instanceId, ItemConfig itemConfig)
        {
            InstanceId = instanceId;
            ItemConfig = itemConfig;
        }
    }
}
