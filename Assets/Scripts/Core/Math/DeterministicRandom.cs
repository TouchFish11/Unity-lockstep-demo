namespace Core.Math
{
    /// <summary>
    /// 确定性伪随机数（xorshift32），锁步逻辑层专用，禁止用 UnityEngine.Random。
    /// </summary>
    public class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            // 0 会让 xorshift 恒为 0，换一个非零默认
            _state = seed == 0 ? 0x9E3779B9u : seed;
        }

        /// <summary>
        /// 返回 [0, 2^32) 的 uint
        /// </summary>
        public uint Next()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>
        /// 返回 [0, 1) 的定点数（Fixed64 16 位小数，取低 16 位）
        /// </summary>
        public Fixed64 NextFixed64() => Fixed64.FromRaw((int)(Next() & 0xFFFF));

        /// <summary>
        /// 返回 [min, max] 的定点数
        /// </summary>
        public Fixed64 Range(Fixed64 min, Fixed64 max) => min + (max - min) * NextFixed64();
    }
}