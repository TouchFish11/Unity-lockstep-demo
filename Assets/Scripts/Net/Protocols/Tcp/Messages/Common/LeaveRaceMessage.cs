using Net.Configs;

namespace Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 离开比赛消息
    /// </summary>
    public class LeaveRaceMessage : TcpMessage
    {
        protected override int GetMsgID()
        {
            return MessageIDConfig.LeaveRace_ID;
        }

        protected override int GetBodyLength()
        {
            return 0;
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {

        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {

        }
    }
}
