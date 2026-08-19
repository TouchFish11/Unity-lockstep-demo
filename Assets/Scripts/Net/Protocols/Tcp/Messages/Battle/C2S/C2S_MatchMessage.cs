using Net.Configs;

namespace Net.Protocols.Tcp.Messages.Battle.C2S
{
    /// <summary>
    /// 匹配消息，是否接受比赛
    /// </summary>
    public class C2S_MatchMessage : TcpMessage
    {
        /// <summary>
        /// 是否匹配，true为接收，false为拒绝
        /// </summary>
        public bool IsMatch { get; set; }

        protected override int GetMsgID()
        {
            return MessageIDConfig.C2S_Match_ID;
        }

        protected override int GetBodyLength()
        {
            return 1;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, IsMatch, ref index);
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            IsMatch = MessageUtil.ReadBool(bytes, ref index);
        }
    }
}
