using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols.Tcp.Handlers
{
    /// <summary>
    /// 服务器离开比赛消息处理器
    /// </summary>
    public class S2C_LeaveRaceMessageHandler : MessageHandler<LeaveRaceMessage>
    {
        public override LeaveRaceMessage Message { get; protected set; }
        
        protected override void OnHandle()
        {
            //
            // if (NetGameManager.Instance.TryGetPlayer(TcpMessage.ClientID, out var _))
            // {
            //     if (TcpMessage.ClientID == NetManager.Instance.ClientID)
            //     {
            //         GameHandler.Instance.IsStop = true;
            //     }
            //     Debug.Log($"��ң�{TcpMessage.ClientID}���˳���Ϸ");
            // } 
        }
    }
}
