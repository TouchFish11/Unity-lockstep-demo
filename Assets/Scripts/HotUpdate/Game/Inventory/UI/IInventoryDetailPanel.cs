using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using UnityEngine;

namespace HotUpdate.Game.Inventory.UI
{
    public interface IInventoryDetailPanel
    {
         GameObject DetailPanel { get; }
        
        void UpdateInfo(ItemConfig itemConfig, ItemData itemData);
    }
}
