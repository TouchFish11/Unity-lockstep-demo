using Core.UI;
using Core.UI.MVC;
using TMPro;
using UnityEngine.UI;

namespace HotUpdate.Main.UI
{
    /// <summary>
    /// 主界面
    /// </summary>
    public class MainPanel : UIView
    {
        [Core.DI.Inject] public TextMeshProUGUI txtFrameRate;
        [Core.DI.Inject] public TextMeshProUGUI txtRTT;

        [Core.DI.Inject] public ScrollRect svFriends;
        [Core.DI.Inject] public ScrollRect svChat;

        [Core.DI.Inject] public InputField inputField;
        
        [Core.DI.Inject] public Button btnSend;
        [Core.DI.Inject]  public Button btnMatch;
    }
}
