using Net.Configs;

namespace Net.Protocols.Tcp.Messages.Battle.S2C
{
    /// <summary>
    /// 开始比赛消息
    /// </summary>
    public class S2C_StartRaceMessage : TcpMessage
    {
        protected override int GetMsgID()
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
