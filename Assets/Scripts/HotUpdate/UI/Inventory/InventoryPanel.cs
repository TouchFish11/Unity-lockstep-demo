using Core.UI;
using Core.UI.MVC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.UI.Inventory
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
        [InjectUI] public Button btnRequestDelete;
        [InjectUI] public Button btnDelete;
        [InjectUI] public Button btnCancelDelete;
        [InjectUI] public TMP_InputField inputFieldDeleteNum;
        [InjectUI] public Button btnSub;
        [InjectUI] public Button btnAdd;
        [InjectUI] public Button btnMin;
        [InjectUI] public Button btnMax;
        [InjectUI] public Slider sliderNum;
        
        [InjectUI(1)] public RectTransform deleteBox;
        
        [InjectUI(1)] public RectTransform DetailArea { get; private set; }
        
        [InjectUI(1)] public RectTransform DeleteArea { get; private set; }
        
        public ToggleGroup OptGroup {get; private set;}

        protected override void Awake()
        {
            base.Awake();
            OptGroup = svOpts.content.GetComponent<ToggleGroup>();
            DeleteArea.gameObject.SetActive(false);
        }
        
        public void SetDeleteBoxActive(bool active)
        {
            deleteBox.gameObject.SetActive(active);
        }
    }
}
