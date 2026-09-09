using Core.Math;

namespace HotUpdate.Game.Race.Logic
{
    /// <summary>
    /// 确定性 AI：在边界内随机选点游走，到达后再选新点
    /// </summary>
    public class AiController
    {
        private readonly LogicAvatar _avatar;
        private readonly DeterministicRandom _random;
        private readonly Fixed64 _reachThreshold;
        private readonly Fixed64 _minX, _maxX, _minZ, _maxZ;
        private FixedVector3 _target;
        private bool _hasTarget;

        public AiController(LogicAvatar avatar, DeterministicRandom random, FixedVector3 minBounds, FixedVector3 maxBounds)
        {
            _avatar = avatar;
            _random = random;
            _reachThreshold = Fixed64.FromFloat(0.2f);
            _minX = minBounds.x; 
            _maxX = maxBounds.x;
            _minZ = minBounds.z; 
            _maxZ = maxBounds.z;
        }

        public void Tick()
        {
            // 没目标，或已走到目标附近 → 重新选点
            if (!_hasTarget || (_target - _avatar.Position).SqrMagnitude() <= _reachThreshold * _reachThreshold)
            {
                PickNewTarget();
            }

            var dir = _target - _avatar.Position;
            if (dir.SqrMagnitude() == Fixed64.Zero)
            {
                _avatar.Move(FixedVector3.Zero);   // 已在目标点 → 站定(Idle)
                return;
            }
            _avatar.Move(dir.Normalized());         // 归一化后交给 Move，恒定速度
        }

        private void PickNewTarget()
        {
            _target = new FixedVector3(_random.Range(_minX, _maxX), Fixed64.Zero, _random.Range(_minZ, _maxZ));
            _hasTarget = true;
        }
    }
}