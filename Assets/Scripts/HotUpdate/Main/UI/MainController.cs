using System.Threading.Tasks;
using Core.Service;
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
                var netGameProxy = ServiceLocator.Get<INetGameProxy>();
                var chatMessage = new ChatMessage
                {
                    ChatMsg = model.GetChatInput()
                };
                netGameProxy.Send(chatMessage, EProtocolChannel.Reliable);
            }
        }
    }
}
