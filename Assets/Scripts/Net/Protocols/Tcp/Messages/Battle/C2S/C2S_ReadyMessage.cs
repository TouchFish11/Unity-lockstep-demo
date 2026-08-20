using Net.Protocols.Configs;

namespace Net.Protocols.Tcp.Messages.Battle.C2S
{
    /// <summary>
    /// 客户端比赛准备就绪消息
    /// </summary>
    public class C2S_ReadyMessage : TcpMessage
    {
        protected override int GetMsgID()
        {
            return MessageIDConfig.C2S_Ready_ID;
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
