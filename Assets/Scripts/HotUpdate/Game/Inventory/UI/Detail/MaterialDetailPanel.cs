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
    public class MaterialDetailPanel : InventoryDetailPanel
    {
        [InjectUI] private TextMeshProUGUI txtMaterialName;
        [InjectUI] private TextMeshProUGUI txtMaterialDescription;
        
        protected override void OnUpdateInfo()
        {
            txtMaterialName.text = itemConfig.name;
            txtMaterialDescription.text = itemConfig.description;
        }
    }
}
