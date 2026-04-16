using Core.UI;
using Core.UI.MVC;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.Game.Inventory.UI
{
    /// <summary>
    /// 背包界面
    /// </summary>
    public class InventoryPanel : UIView
    {
        [InjectUI] public ScrollRect svOpts;
        [InjectUI] public ScrollRect svItems;
        [InjectUI(1)] public RectTransform DetailArea { get; private set; }
        [InjectUI] public Button btnClose;
    }
}
