using Core.UI;
using HotUpdate.Common.Items.Config;
using HotUpdate.Common.Items.Data;
using UnityEngine;

namespace HotUpdate.UI.Inventory
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
