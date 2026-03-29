using Core.DI;
using Core.UI;
using Net.Sync;
using TMPro;

namespace HotUpdate.Main.Chat
{
    public class ChatUI : UIBehaviourBase
    {
        [Core.DI.Inject] private TextMeshProUGUI txtChatMsg;
        
        public void SetMessage(int clientID, string msg)
        {
            var userName = clientID == DIContainer.GetInstance<INetGameProxy>().SessionId ? "我" : $"{clientID}";
            txtChatMsg.text = $"{userName}：{msg}";
        }
    }
}
