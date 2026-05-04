using HotUpdate.Common.Items.Config;
using HotUpdate.Common.Items.Data;
using UnityEngine;

namespace HotUpdate.UI.Inventory
{
    public interface IInventoryDetailPanel
    {
         GameObject DetailPanel { get; }
        
        void UpdateInfo(ItemConfig itemConfig, ItemData itemData);
    }
}
