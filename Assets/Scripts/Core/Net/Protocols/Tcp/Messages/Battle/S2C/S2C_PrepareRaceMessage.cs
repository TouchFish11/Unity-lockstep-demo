using System.Collections.Generic;
using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Battle.S2C
{
    /// <summary>
    /// 准备比赛消息，客户端加载、初始化比赛相关资源
    /// </summary>
    [MessageDir(EMessageHandle.Resolve)]
    public class S2C_PrepareRaceMessage : TcpMessage
    {
        /// <summary>
        /// 比赛ID，客户端可以本地缓存比赛ID，用于断线重连时重新加入比赛
        /// </summary>
        public int RaceID { get; set; }
        
        // 当前同一比赛的所有客户端ID
        public List<int> clientIds;

        public override int GetMsgID()
        {
            return MessageIDConfig.S2C_PrepareRace_ID;
        }

        protected override int GetBodyLength()
        {
            // 比赛ID + clientIds长度 + clientIds主体
            return 4 + 4 + 4 * clientIds.Count;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, RaceID, ref index);
            MessageUtil.WriteField(bytes, clientIds.Count, ref index);
            foreach (var clientId in clientIds)
            {
                MessageUtil.WriteField(bytes, clientId, ref index);
            }
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            RaceID = MessageUtil.ReadInt(bytes, ref index);
            var length = MessageUtil.ReadInt(bytes, ref index);
            clientIds = new List<int>(length);
            for (var i = 0; i < length; i++)
            {
                clientIds.Add(MessageUtil.ReadInt(bytes, ref index));
            }
        }
    }
}
