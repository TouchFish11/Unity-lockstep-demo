using Core.UI;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using TMPro;
using UnityEngine;

namespace HotUpdate.Game.Inventory.UI.Detail
{
    /// <summary>
    /// 材料类型详细界面
    /// </summary>
    public class MaterialDetailPanel : UIBehaviourBase, IInventoryDetailPanel
    {
        [InjectUI] private TextMeshProUGUI txtMaterialName;
        [InjectUI] private TextMeshProUGUI txtMaterialDescription;

        public GameObject DetailPanel => this.gameObject;

        public void UpdateInfo(ItemConfig itemConfig, ItemData itemData)
        {
            txtMaterialName.text = itemConfig.name;
            txtMaterialDescription.text = itemConfig.description;
        }
    }
}
