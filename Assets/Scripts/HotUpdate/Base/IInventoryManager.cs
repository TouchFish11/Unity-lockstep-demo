using System.Collections.Generic;
using System.Threading.Tasks;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;

namespace HotUpdate.Base
{
    public interface IInventoryManager
    {
        /// <summary>
        /// 加载物品配置
        /// </summary>
        Task LoadItemConfig();

        List<ItemData> GetItemDataByType(EItemType itemType);
        
        Task<ItemDTO> CreateItemDTO(ItemConfig itemConfig, ItemData itemData);
        
        IEnumerable<ItemData> GetItems();
    }
}
