using System.Threading.Tasks;
using Core.DI;
using Core.UI.MVC;
using Net.Sync;
using Net.Sync.Msg;
using Net.Sync.Msg.Chat;

namespace HotUpdate.Main.UI
{
    public class MainController : UIController<MainPanel, MainModel>
    {
        protected override Task OnShow()
        {
            return Task.CompletedTask;
        }

        protected override Task OnHide()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInit()
        {
            return Task.CompletedTask;
        }

        public void AddChat(int sessionId, string chatMsg)
        {
            //_prefabLoader.GetObjectAsync<ChatUI>()
            //var chatObj = _editorResManager.LoadEditorAsset<GameObject>(ResKeyCollection.ChatUI);
            //chatObj.transform.SetParent(view.svChat.content, false);
            //var chatUI = chatObj.GetComponent<ChatUI>();
            //chatUI.SetMessage(sessionId, chatMsg);
            //model.Cache(chatUI);
        }

        protected override void InputFieldValueChanged(string fieldName, string inputStr)
        {
            if (fieldName == nameof(view.inputField))
            {
                model.InputStr(inputStr);
            }
        }

        protected override void ButtonOnClick(string btnName)
        {
            if (btnName == nameof(view.btnMatch))
            {
                DIContainer.GetInstance<INetManager>().Send(new MatchGameMessage(), EProtocolChannel.Reliable);
            }
            else if (btnName == nameof(view.btnSend))
            {
                var netGameProxy = DIContainer.GetInstance<INetGameProxy>();
                var chatStr = model.GetChatInput();
                
                // 本地创建自己发送的消息
                AddChat(netGameProxy.SessionId, chatStr);
                
                var chatMessage = new ChatMessage
                {
                    ChatMsg = chatStr
                };
                // 发送消息
                netGameProxy.Send(chatMessage, EProtocolChannel.Reliable);
            }
        }
    }
}
