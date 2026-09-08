using System;
using Core.Net;
using UnityEngine;

namespace Core.Global.Configs
{
    /// <summary>
    /// 网络模块配置
    /// </summary>
    [Serializable]
    public class NetModuleConfig
    {
        /// <summary>
        /// 网络客户端配置
        /// </summary>
        public NetConfig netConfig;
        
        /// <summary>
        /// UDP接收缓冲区大小
        /// </summary>
        [Header("UDP接收缓冲区大小")]
        [Tooltip("缓存帧同步数据包")]
        [Obsolete]
        public short udpReceiveBufferSize = 8192;

        /// <summary>
        /// TCP接收临时缓冲区大小
        /// </summary>
        [Header("TCP发送缓冲区大小")]
        [Tooltip("临时缓存接收的TCP消息")]
        public short tcpReceiveTempBufferSize = 1024;

        /// <summary>
        /// TCP接收缓冲区大小
        /// </summary>
        [Header("TCP接收缓冲区大小")]
        [Tooltip("缓存接收的TCP待处理消息")]
        public short tcpReceiveBufferSize = 1024;

        /// <summary>
        /// 心跳消息发送间隔时间（ms）
        /// </summary>
        [Header("心跳消息发送间隔时间（ms）")]
        [Tooltip("心跳消息发送间隔（ms）")]
        public short heartMsgSendIntervalTime = 5000;
        
        /// <summary>
        /// 心跳超时阈值（ms）
        /// </summary>
        [Header("心跳超时阈值（ms）")]
        [Tooltip("当前时间与上次心跳时间的差值超过该阈值，视为连接超时")]
        public int heartTimeoutThreshold = 15000;
    }
}
