using Net.Protocols.Configs;

namespace Net.Protocols.Tcp.Messages.Battle.C2S
{
    /// <summary>
    /// 请求匹配消息
    /// </summary>
    public class C2S_MatchMessage : TcpMessage
    {
        protected override int GetMsgID()
        {
            return MessageIDConfig.C2S_Match_ID;
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
