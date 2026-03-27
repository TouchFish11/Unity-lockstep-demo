using System.Threading.Tasks;
using Core.DI;
using Core.EditorRes;
using Core.Loader.Object;
using Core.Service;
using Core.UI.MVC;
using HotUpdate.Main.Chat;
using Net.Sync;
using Net.Sync.Msg;
using Net.Sync.Msg.Chat;
using UnityEngine;

namespace HotUpdate.Main.UI
{
    public class MainController : UIController<MainPanel, MainModel>
    {
        private readonly IPrefabLoader _prefabLoader = DIContainer.GetDependency<IPrefabLoader>();
        private readonly IEditorResManager _editorResManager = DIContainer.GetDependency<IEditorResManager>();
        
        protected override Task OnShow()
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnHide()
        {
            throw new System.NotImplementedException();
        }

        protected override Task OnInit()
        {
            throw new System.NotImplementedException();
        }

        public void AddChat(int sessionId, string chatMsg)
        {
            var chatObj = _editorResManager.LoadEditorAsset<GameObject>(nameof(ChatUI));
            chatObj.transform.SetParent(view.svChat.content, false);
            var chatUI = chatObj.GetComponent<ChatUI>();
            chatUI.SetMessage(sessionId, chatMsg);
            model.Cache(chatUI);
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
                ServiceLocator.Get<INetManager>().Send(new MatchGameMessage(), EProtocolChannel.Reliable);
            }
            else if (btnName == nameof(view.btnSend))
            {
                var netGameProxy = DIContainer.GetDependency<INetGameProxy>();
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
