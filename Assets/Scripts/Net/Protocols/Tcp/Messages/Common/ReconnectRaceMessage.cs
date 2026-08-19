using Net.Configs;

namespace Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 重新连接比赛消息
    /// </summary>
    public class ReconnectRaceMessage : TcpMessage
    {
        protected override int GetMsgID()
        {
            return MessageIDConfig.ReconnectRace_ID;
        }

        protected override int GetBodyLength()
        {
            return 0;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {

        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {

        }
    }
}
