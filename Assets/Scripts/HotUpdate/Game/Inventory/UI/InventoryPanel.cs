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
        [InjectUI] public Dropdown dpSorts;
        [InjectUI] public Button btnClose;
        
        [InjectUI(1)] public RectTransform DetailArea { get; private set; }
        
        public ToggleGroup OptGroup {get; private set;}

        protected override void Awake()
        {
            base.Awake();
            OptGroup = svOpts.content.GetComponent<ToggleGroup>();
        }
    }
}
