using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Battle.S2C
{
    /// <summary>
    /// 开始比赛消息
    /// </summary>
    [MessageDir(EMessageHandle.Resolve)]
    public class S2C_StartRaceMessage : TcpMessage
    {
        public override int GetMsgID()
        {
            return MessageIDConfig.S2C_StartRace_ID;
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
