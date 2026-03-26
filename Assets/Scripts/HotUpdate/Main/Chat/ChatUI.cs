using Core.UI;
using TMPro;

namespace HotUpdate.Main.Chat
{
    public class ChatUI : UIBehaviourBase
    {
        [Inject] private TextMeshProUGUI txtChatMsg;
        
        public void SetMessage(int clientID, string msg)
        {
            txtChatMsg.text = $"{clientID}：{msg}";
        }
    }
}
