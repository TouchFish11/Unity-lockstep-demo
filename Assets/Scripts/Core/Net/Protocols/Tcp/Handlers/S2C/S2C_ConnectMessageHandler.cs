using Core.DI;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;

namespace Core.Net.Protocols.Tcp.Handlers.S2C
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
            var connectResult = new ConnectResult
            {
                SessionId = Message.SessionID,
                SessionIds = Message.ClientIds.ToArray(),
                RaceExist = Message.RaceExist,
                RaceIds = Message.CurrentRaceIds.ToArray(),
            };
            
            ((NetManager)netManager).SetConnectStatus(connectResult);
            // 发送心跳
            _heartbeatService.Start();
        }
    }
}
