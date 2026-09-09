using System;
using Core.Net.kcp2k.highlevel;
using Core.Net.Protocols;
using Core.Net.SyncModule.Manager;
using UnityEngine;

namespace Core.Net
{
    [Serializable]
    public class NetConfig
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string serverIp;

        /// <summary>
        /// 服务器端口
        /// </summary>
        public ushort serverPort;

        /// <summary>
        /// 消息序列化器
        /// </summary>
        [SerializeReference] public IMessageResolver resolver;

        /// <summary>
        /// 协议类型
        /// </summary>
        public EClientType clientType;

        /// <summary>
        /// 专属Kcp配置，其它协议忽略此属性，当协议类型为Kcp时使用此属性，若为空则使用默认的kcp配置
        /// </summary>
        public KcpConfig kcpConfig;

        public override string ToString()
        {
            string kcp = $"{kcpConfig.Interval}";
            return $"{serverIp}:{serverPort},{clientType},{kcp}";
        }
    }
}
