using System;
using Net.Configs;
using Net.Protocols.Tcp.Messages.Battle.S2C;
using Net.Protocols.Tcp.Messages.Common;

namespace Net.Protocols
{
    /// <summary>
    /// 消息工厂
    /// </summary>
    public static class MessageFactory
    {
        /// <summary>
        /// 创建消息
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="bytes"></param>
        /// <param name="nowIndex"></param>
        /// <returns></returns>
        public static Message CreateMessage(int msgId, byte[] bytes, int nowIndex)
        {
            Message message = msgId switch
            {
                MessageIDConfig.S2C_Connect_ID => new S2C_ConnectMessage(),
                MessageIDConfig.S2C_Disconnect_ID => new S2C_DisConnectMessage(),
                MessageIDConfig.S2C_MatchSuccess_ID => new S2C_MatchSuccessMessage(),
                MessageIDConfig.S2C_PrepareRace_ID => new S2C_PrepareRaceMessage(),
                MessageIDConfig.S2C_StartRace_ID => new S2C_StartRaceMessage(),
                MessageIDConfig.Heartbeat_ID => new HeartMessage(),
                MessageIDConfig.LeaveRace_ID => new LeaveRaceMessage(),
                MessageIDConfig.ReconnectRace_ID => new ReconnectRaceMessage(),
                _ => throw new Exception($"Unknown message id: {msgId}")
            };

            // 序列化消息体
            message.Deserialize(bytes, nowIndex);
            return message;
        }

        public static int GetMessageID(Message message)
        {
            if (message is S2C_ConnectMessage)
            {
                return 2001;
            }
            
            throw new ArgumentOutOfRangeException(message.ToString());
        }
    }
}
