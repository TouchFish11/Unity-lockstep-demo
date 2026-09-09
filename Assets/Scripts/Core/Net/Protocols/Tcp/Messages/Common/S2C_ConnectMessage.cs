using System.Collections.Generic;
using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.Tcp.Messages.Common
{
    /// <summary>
    /// 连接消息
    /// <remarks>
    /// 服务器发送给所有客户端，需要设置<see cref="TcpMessage.SessionID"/>，并 携带其它客户端的ID<see cref="ClientIds"/>。
    /// </remarks>
    /// </summary>
    [MessageDir(EMessageHandle.Resolve)]
    public class S2C_ConnectMessage : TcpMessage
    {
        /// <summary>
        /// 是否存在比赛尚未结束，存在则处理重连逻辑
        /// </summary>
        public bool RaceExist { get; set; }
        
        /// <summary>
        /// 所有已连接的客户端ID，包括发送目标客户端
        /// </summary>
        public List<int> ClientIds { get; set; }
        
        /// <summary>
        /// 若比赛尚未结束，则返回当前比赛的所有客户端的比赛ID列表
        /// </summary>
        public List<int> CurrentRaceIds { get; set; }

        public override int GetMsgID()
        {
            return MessageIDConfig.S2C_Connect_ID;
        }
        
        protected override int GetBodyLength()
        {

            return 1 +
                   4 + 4 * ClientIds.Count + // ClientIds
                   4 + 4 * CurrentRaceIds.Count;
        }

        protected override void SerializeBody(byte[] bytes, ref int index)
        {
            MessageUtil.WriteField(bytes, RaceExist, ref index);
            // 序列化客户端列表
            MessageUtil.WriteField(bytes, ClientIds.Count, ref index);
            foreach (var clientId in ClientIds)
            {
                MessageUtil.WriteField(bytes, clientId, ref index);
            }
            
            // 序列化比赛ID列表
            MessageUtil.WriteField(bytes, CurrentRaceIds.Count, ref index);
            foreach (var clientId in CurrentRaceIds)
            {
                MessageUtil.WriteField(bytes, clientId, ref index);
            }
        }

        protected override void DeserializeBody(byte[] bytes, ref int index)
        {
            RaceExist = MessageUtil.ReadBool(bytes, ref index);
            var count = MessageUtil.ReadInt(bytes, ref index);
            ClientIds = new List<int>();
            for (var i = 0; i < count; i++)
            {
                ClientIds.Add(MessageUtil.ReadInt(bytes, ref index));
            }
            
            var raceIdCount = MessageUtil.ReadInt(bytes, ref index);
            CurrentRaceIds = new List<int>();
            for (var i = 0; i < raceIdCount; i++)
            {
                CurrentRaceIds.Add(MessageUtil.ReadInt(bytes, ref index));
            }
        }
    }
}
