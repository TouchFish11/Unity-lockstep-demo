using Core.Math;
using Core.Net.Protocols.FSync;

namespace HotUpdate.Game.Race.Logic
{
    public class LogicAvatar
    {
        private static readonly Fixed64 LogicDeltaTime = Fixed64.FromFloat(0.066f); // 66ms

        public int PlayerId { get; }
        public FixedVector3 Position { get; private set; }
        public FixedVector3 PrevPosition { get; private set; }
        public ELogicAnimState AnimState { get; private set; }
        public int Version { get; private set; }
        private readonly Fixed64 _speed;

        public LogicAvatar(int playerId, Fixed64 speed)
        {
            PlayerId = playerId;
            _speed = speed;
            AnimState = ELogicAnimState.Idle;
        }

        public void Execute(in InputCommand cmd)
        {
            switch (cmd.optType)
            {
                case EOptType.Move:
                    Move(cmd.dir);
                    break;
                case EOptType.Attack:
                    Attack(cmd.targetId);
                    break;
                case EOptType.UseSkill:
                    break; // 本期不实现
            }
        }

        public void Move(FixedVector3 dir)
        {
            if (dir.SqrMagnitude() == Fixed64.Zero)
            {
                AnimState = ELogicAnimState.Idle;
                return;
            }

            PrevPosition = Position;
            // 收到的dir已经是单位向量的定点数，是否还需要归一化这里？
            Position += dir * _speed * LogicDeltaTime;
            AnimState = ELogicAnimState.Move;
            // 位置变了，版本号 +1
            Version++;                              
        }

        public void Attack(int targetId)
        {
            AnimState = ELogicAnimState.Attack; // 本期只切状态
        }
    }
}