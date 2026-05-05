using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.DI;
using Core.Net.FrameSync.Manager;
using Core.UI.ViewController;
using HotUpdate.Game.Main.Chat;
using HotUpdate.Main.Chat;
using HotUpdate.Main.UI;

namespace HotUpdate.Game.Main.UI
{
    public class MainController : UIController<MainPanel>
    {
        private List<FriendUI> friendUis = new();
        private readonly List<ChatUI> chatUis = new();
        private readonly StringBuilder _currentInput = new(16);
        
        protected override Task OnActive()
        {
            return Task.CompletedTask;
        }

        protected override Task OnInactivate()
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
        
        public void InputStr(string str)
        {
            _currentInput.Append(str);
        }

        public void Cache(ChatUI chatUI)
        {
            chatUis.Add(chatUI);
        }
        
        public string GetChatInput() => _currentInput.ToString();

        protected override void OnInputFieldValueChanged(string fieldName, string inputStr)
        {
            if (fieldName == nameof(view.inputField))
            {
                InputStr(inputStr);
            }
        }
        
        protected override void OnButtonClick(string btnName)
        {
            // if (btnName == nameof(view.btnMatch))
            // {
            //     DIContainer.GetInstance<INetManager>().Send(new MatchGameMessage(), EProtocolChannel.Reliable);
            // }
            // else if (btnName == nameof(view.btnSend))
            // {
            //     var netGameProxy = DIContainer.GetInstance<INetGameProxy>();
            //     var chatStr = GetChatInput();
            //     
            //     // 本地创建自己发送的消息
            //     AddChat(netGameProxy.SessionId, chatStr);
            //     
            //     var chatMessage = new ChatMessage
            //     {
            //         ChatMsg = chatStr
            //     };
            //     // 发送消息
            //     netGameProxy.Send(chatMessage, EProtocolChannel.Reliable);
            // }
        }
    }
}
