using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 客户端通知服务器比赛结束（胜负已定），服务器清空比赛状态允许重新匹配
    /// </summary>
    public class C2S_RaceEndMessage : TcpMessage
    {
        public override int GetMsgID()
        {
            return MessageIDConfig.C2S_RaceEnd_ID;
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