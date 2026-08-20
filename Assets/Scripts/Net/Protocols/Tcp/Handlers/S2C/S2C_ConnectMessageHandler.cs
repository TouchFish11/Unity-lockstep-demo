using Core.DI;
using Net.Protocols.Tcp.Messages.Common;
using Net.SyncModule.Interface;

namespace Net.Protocols.Tcp.Handlers.S2C
{
    /// <summary>
    /// 服务器连接消息处理器
    /// </summary>
    public class S2C_ConnectMessageHandler : MessageHandler<S2C_ConnectMessage>
    {
        [Inject] private IHeartbeatService _heartbeatService;
        
        public override S2C_ConnectMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            // 记录ID
            _netGameProxy.SetSessionToken(Message.SessionID);
            // 发送心跳
            _heartbeatService.Start();
        }
    }
}
