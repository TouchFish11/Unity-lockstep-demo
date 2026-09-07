using Core.Math;

namespace Core.Net.Protocols.FSync
{
    /// <summary>
    /// 输入命令结构
    /// </summary>
    public struct InputCommand
    {
        public int playerId;
        public EOptType optType;
        public FixedVector3 dir;
        public int targetId;
        public int skillId;
    }
}
