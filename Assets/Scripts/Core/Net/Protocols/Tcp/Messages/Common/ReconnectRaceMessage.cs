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
        /// 当前比赛的ID，用于重连
        /// </summary>
        public int RaceId { get; set; }
        
        public override int GetMsgID()
        {
            return MessageIDConfig.ReconnectRace_ID;
        }

        protected override int GetBodyLength()
        {
            return 4;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, RaceId, ref index);
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            RaceId = MessageUtil.ReadInt(bytes, ref index);
        }
    }
}
