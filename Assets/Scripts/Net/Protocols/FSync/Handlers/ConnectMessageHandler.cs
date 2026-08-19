using Core.DI;
using Core.Log;
using Net.Protocols.Tcp.Messages.Common;
using Net.SyncModule.Interface;

namespace Net.Protocols.FSync.Handlers
{
    /// <summary>
    /// 连接消息处理
    /// </summary>
    public class ConnectMessageHandler : MessageHandler<S2C_ConnectMessage>
    {
        public override S2C_ConnectMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            // 设置当前连接的会话ID
            DIContainer.GetInstance<INetGameProxy>().SetSessionToken(Message.SessionID);
            Logger.Log($"[ConnectMessageHandler] 已处理连接消息");
        }
    }
}
