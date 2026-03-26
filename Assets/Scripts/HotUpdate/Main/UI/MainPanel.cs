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
        [Inject] public TextMeshProUGUI txtFrameRate;
        [Inject] public TextMeshProUGUI txtRTT;

        [Inject] public ScrollRect svFriends;
        [Inject] public ScrollRect svChat;

        [Inject] public InputField inputField;
        
        [Inject] public Button btnSend;
        [Inject]  public Button btnMatch;
    }
}
