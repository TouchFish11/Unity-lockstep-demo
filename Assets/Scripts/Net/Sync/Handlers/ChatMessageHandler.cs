using Core.Service;
using Core.UI;
using HotUpdate.Main.UI;
using Net.Sync.Msg.Chat;

namespace Net.Sync.Handlers
{
    /// <summary>
    /// 聊天消息处理器
    /// </summary>
    public class ChatMessageHandler : MessageHandler<ChatMessage>
    {
        public override ChatMessage Message { get; protected set; }
        
        protected override void OnHandleMessage()
        {
            var controller = ServiceLocator.Get<IUIManager>().GetController<MainController>();
            controller.AddChat(Message.SessionID, Message.ChatMsg);
        }
    }
}
