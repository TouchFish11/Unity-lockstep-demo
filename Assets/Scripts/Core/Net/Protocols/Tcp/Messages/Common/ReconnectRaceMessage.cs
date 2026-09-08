using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 重新连接比赛消息
    /// </summary>
    [MessageDir(EMessageHandle.Both)]
    public class ReconnectRaceMessage : TcpMessage
    {
        /// <summary>
        /// 重连认领：玩家旧 ID（稳定身份）。SessionID 是自动填的新连接ID
        /// </summary>
        public int PlayerID { get; set; }
        
        public override int GetMsgID()
        {
            return MessageIDConfig.ReconnectRace_ID;
        }

        protected override int GetBodyLength()
        {
            return sizeof(int);
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, PlayerID, ref index);
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            PlayerID = MessageUtil.ReadInt(bytes, ref index);
        }
    }
}
