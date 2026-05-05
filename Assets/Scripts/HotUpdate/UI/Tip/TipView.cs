using Core.UI;
using Core.UI.ViewController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate.UI.Tip
{
    /// <summary>
    /// 提示界面基类
    /// </summary>
    public class TipView : UIView
    {
        [InjectUI] public TextMeshProUGUI txtTipTitle;
        [InjectUI] public TextMeshProUGUI txtTipContent;
        [InjectUI] public Button btnCancel;
        [InjectUI] public Button btnOk;
        
        [InjectUI(1)] public RectTransform ContentRoot { get; private set; }
        
        
        
        
        
    }
}
