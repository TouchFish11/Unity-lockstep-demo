using Core.UI;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using UnityEngine;

namespace HotUpdate.Game.Inventory.UI
{
    public abstract class InventoryDetailPanel : UIBehaviourBase, IInventoryDetailPanel
    {
        protected ItemConfig itemConfig;
        protected ItemData itemData;
        
        public GameObject DetailPanel => this.gameObject;
    
        public void UpdateInfo(ItemConfig itemConfig, ItemData itemData)
        {
            this.itemConfig = itemConfig;
            this.itemData = itemData;
            OnUpdateInfo();
        }
        
        protected abstract void OnUpdateInfo();
    }
}
