using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 离开比赛消息
    /// </summary>
    [MessageDir(EMessageHandle.Both)]
    public class LeaveRaceMessage : TcpMessage
    {
        public override int GetMsgID()
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
