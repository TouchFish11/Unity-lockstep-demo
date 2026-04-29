using Core.UI;
using TMPro;

namespace HotUpdate.Game.Inventory.UI.Detail
{
    public class WeaponDetailPanel : InventoryDetailPanel
    {
        [InjectUI] private TextMeshProUGUI txtWeaponNameInfo;
        [InjectUI] private TextMeshProUGUI txtWeaponDescriptionInfo;
        
        protected override void OnUpdateInfo()
        {
            txtWeaponNameInfo.text = itemConfig.name;
            txtWeaponDescriptionInfo.text = itemConfig.description;
        }
    }
}
