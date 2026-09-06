using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.FSync.Messages
{
    /// <summary>
    /// 客户端请求补发帧消息：从 StartFrame 开始补发
    /// </summary>
    public class C2S_RequestFramesMessage : FrameMessage
    {
        /// <summary>
        /// 请求补发的客户端 ID
        /// </summary>
        public int SessionID { get; set; }

        /// <summary>
        /// 从这一帧开始补发
        /// </summary>
        public int StartFrame { get; set; }

        public int FrameMessageID => MessageIDConfig.C2S_RequestFrames_ID;

        public override int GetMsgLength()
        {
            return sizeof(int) * 2;
        }

        public override byte[] Serialize()
        {
            var index = 0;
            var bytes = new byte[GetMsgLength()];
            MessageUtil.WriteField(bytes, SessionID, ref index);
            MessageUtil.WriteField(bytes, StartFrame, ref index);
            return bytes;
        }

        public override int Deserialize(byte[] bytes, int beginIndex = 0)
        {
            var index = beginIndex;
            SessionID = MessageUtil.ReadInt(bytes, ref index);
            StartFrame = MessageUtil.ReadInt(bytes, ref index);
            return index - beginIndex;
        }
    }
}