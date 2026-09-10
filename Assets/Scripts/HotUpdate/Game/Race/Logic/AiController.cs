using System.Collections.Generic;
using Core.Math;

namespace HotUpdate.Game.Race.Logic
{
    /// <summary>
    /// 确定性 AI：在边界内随机选点游走，到达后再选新点
    /// </summary>
    public class AiController
    {
        private readonly LogicAvatar _avatar;

        public AiController(LogicAvatar avatar)
        {
            _avatar = avatar;
        }

        public void Tick(IReadOnlyList<LogicAvatar> avatars)
        {
            // 死怪物不再行动
            if (_avatar.IsDead || _avatar.IsAttacking)
                return; 
            
            // 找最近的存活玩家
            LogicAvatar nearest = null;
            var nearestDistSq = Fixed64.MaxValue;
            foreach (var other in avatars)
            {
                // 只打玩家，跳过怪物和自己
                if (!other.IsPlayer || other.IsDead)
                    continue;
                
                var d = (other.Position - _avatar.Position).SqrMagnitude();
                if (d < nearestDistSq)
                {
                    nearestDistSq = d;
                    nearest = other;
                }
                
                // 没有存活玩家
                if (nearest == null)
                {
                    _avatar.Move(FixedVector3.Zero);
                }
                
                var dir = nearest.Position - _avatar.Position;
                var rangeSq = LogicAvatar.AttackRange * LogicAvatar.AttackRange;
                if (dir.SqrMagnitude() <= rangeSq)
                {
                    _avatar.FaceToward(dir);
                    _avatar.StartAttack();
                }
                else
                {
                    // 归一化后交给 Move，恒定速度
                    _avatar.Move(dir.Normalized());
                }
            }
        }
    }
}