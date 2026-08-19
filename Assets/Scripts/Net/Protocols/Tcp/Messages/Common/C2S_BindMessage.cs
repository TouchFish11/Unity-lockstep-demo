using System;
using Net.Configs;

namespace Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 绑定消息
    /// <remarks>
    /// ID：<see cref="MessageIDConfig.Bind_ID"/>
    /// </remarks>
    /// </summary>
    public class C2S_BindMessage : TcpMessage
    {
        [Obsolete("不在需要传递端口")]
        public int UdpPort { get; set; }

        protected override int GetMsgID()
        {
            return MessageIDConfig.C2S_Bind_ID;
        }

        protected override int GetBodyLength()
        {
            return 4;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, UdpPort, ref index);
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            UdpPort = MessageUtil.ReadInt(bytes, ref index);
        }

        public override string ToString()
        {
            return $"��ң�{SessionID}���Ѷ�̬�󶨿ͻ��˶˿ڣ�{UdpPort}";
        }
    }
}
