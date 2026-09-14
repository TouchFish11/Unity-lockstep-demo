namespace HotUpdate.Game.Race.Logic
{
    /// <summary>
    /// 帧同步全局常量（单一时间源，改帧率只动这里）
    /// </summary>
    public static  class FrameSyncConfig
    {
        /// <summary>
        /// 每逻辑帧的秒数（15Hz 锁步，对齐服务器 66ms tick）。
        /// 逻辑层用 Fixed64.FromFloat 转定点，表现层直接用 float。
        /// </summary>
        public const float LogicFrameSeconds = 0.066f;
    }
}
