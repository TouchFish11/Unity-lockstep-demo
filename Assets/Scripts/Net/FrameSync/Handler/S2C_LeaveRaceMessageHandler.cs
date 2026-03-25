using Net.FrameSync.Manager;
using Net.FrameSync.Tcp.Message;
using Net.FrameSync.Tcp.Message.S2C;
using Net.FrameSync.Test;
using UnityEngine;


namespace Net.FrameSync.Handler
{
    public class S2C_LeaveRaceMessageHandler : MessageHandler<S2C_LeaveRaceMessage>
    {
        public override S2C_LeaveRaceMessage TcpMessage { get; set; }

        public override void HandleMessage(TcpMessage tcpMessage)
        {
            base.HandleMessage(tcpMessage);

            if (NetGameManager.Instance.TryGetPlayer(TcpMessage.ClientID, out var _))
            {
                if (TcpMessage.ClientID == NetManager.Instance.ClientID)
                {
                    GameHandler.Instance.IsStop = true;
                }
                Debug.Log($"��ң�{TcpMessage.ClientID}���˳���Ϸ");
            }
        }
    }
}
