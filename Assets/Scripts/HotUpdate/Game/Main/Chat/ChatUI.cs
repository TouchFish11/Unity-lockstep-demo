using Core.DI;
using Core.UI;
using TMPro;

namespace HotUpdate.Game.Main.Chat
{
    public class ChatUI : UIBehaviourBase
    {
        [Core.DI.Inject] private TextMeshProUGUI txtChatMsg;
        
        public void SetMessage(int clientID, string msg)
        {
            //var userName = clientID == DIContainer.GetInstance<INetGameProxy>().SessionId ? "我" : $"{clientID}";
            //txtChatMsg.text = $"{userName}：{msg}";
        }
    }
}
