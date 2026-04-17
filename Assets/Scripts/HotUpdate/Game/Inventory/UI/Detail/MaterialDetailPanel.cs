using Core.UI;
using TMPro;

namespace HotUpdate.Game.Inventory.UI.Detail
{
    /// <summary>
    /// 材料类型详细界面
    /// </summary>
    public class MaterialDetailPanel : UIBehaviourBase
    {
        [InjectUI] private TextMeshProUGUI txtMaterialName;
        [InjectUI] private TextMeshProUGUI txtMaterialDescription;

        public void UpdateInfo(string materialName, string materialDescription)
        {
            txtMaterialName.text = materialName;
            txtMaterialDescription.text = materialDescription;
        }
    }
}
