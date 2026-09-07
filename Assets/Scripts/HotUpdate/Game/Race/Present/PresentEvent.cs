using Core.Math;

namespace HotUpdate.Game.Race.Present
{
    public enum EPresentEventType : byte
    {
        SpawnEffect = 0,
        PlaySound = 1
    }
    
    /// <summary>
    /// 表现事件
    /// </summary>
    public struct PresentEvent
    {
        public EPresentEventType Type;
        public int Id;             // 特效/音效 ID
        public FixedVector3 Pos;
    }
}
