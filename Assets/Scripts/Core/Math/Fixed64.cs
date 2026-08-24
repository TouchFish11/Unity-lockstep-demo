using System;

namespace Core.Math
{
    public readonly struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>
    {
        private const int FractionalBits = 16;          // 小数位数
        private const long OneRaw = 1 << FractionalBits; // 1 的定点表示
        public static readonly Fixed64 Zero = new(0);
        public static readonly Fixed64 One = new(OneRaw);
        public static readonly Fixed64 Half = new(OneRaw / 2);
        public static readonly Fixed64 MaxValue = new(long.MaxValue);
        public static readonly Fixed64 MinValue = new(long.MinValue);
        
        private readonly long _rawValue; // 实际值 = _rawValue / 65536
        
        private Fixed64(long raw)
        {
            _rawValue = raw;
        }

        // 从原始定点值创建（内部使用）
        public static Fixed64 FromRaw(long raw) => new(raw);

        // 从 int 创建（自动转换为定点数）
        public static Fixed64 FromInt(int value) => new((long)value << FractionalBits);

        // 从 float 创建（仅用于配置，逻辑中禁止使用）
        public static Fixed64 FromFloat(float value)
        {
            return new Fixed64((long)(value * OneRaw));
        }

        // 从 double 创建（同上）
        public static Fixed64 FromDouble(double value)
        {
            return new Fixed64((long)(value * OneRaw));
        }

        /// <summary>
        /// 定点数（用于序列化等）
        /// </summary>
        public long RawValue => _rawValue;
        
        public float ToFloat() => (float)_rawValue / OneRaw;
        public double ToDouble() => (double)_rawValue / OneRaw;
        public int ToInt() => (int)(_rawValue >> FractionalBits);
        public long ToLong() => _rawValue >> FractionalBits;
        
        public static Fixed64 operator +(Fixed64 a, Fixed64 b) => new(a._rawValue + b._rawValue);
        public static Fixed64 operator -(Fixed64 a, Fixed64 b) => new(a._rawValue - b._rawValue);
        public static Fixed64 operator -(Fixed64 a) => new(-a._rawValue);

        public static Fixed64 operator *(Fixed64 a, Fixed64 b)
        {
            // 64位 * 64位 = 128位，右移16位保留精度
            // 使用 long 乘法可能溢出，因此拆分为两个部分处理或使用 Math.BigMul
            // 简单起见，这里假设输入范围不会导致溢出，直接相乘再右移
            long product = a._rawValue * b._rawValue;
            return new Fixed64(product >> FractionalBits);
        }

        public static Fixed64 operator /(Fixed64 a, Fixed64 b)
        {
            // 先左移16位提升精度，再除法
            long numerator = a._rawValue << FractionalBits;
            return new Fixed64(numerator / b._rawValue);
        }

        public static bool operator ==(Fixed64 a, Fixed64 b) => a._rawValue == b._rawValue;
        public static bool operator !=(Fixed64 a, Fixed64 b) => a._rawValue != b._rawValue;
        public static bool operator <(Fixed64 a, Fixed64 b) => a._rawValue < b._rawValue;
        public static bool operator >(Fixed64 a, Fixed64 b) => a._rawValue > b._rawValue;
        public static bool operator <=(Fixed64 a, Fixed64 b) => a._rawValue <= b._rawValue;
        public static bool operator >=(Fixed64 a, Fixed64 b) => a._rawValue >= b._rawValue;
        
        public bool Equals(Fixed64 other) => _rawValue == other._rawValue;
        public override bool Equals(object obj) => obj is Fixed64 f && Equals(f);
        public override int GetHashCode() => _rawValue.GetHashCode();
        public int CompareTo(Fixed64 other) => _rawValue.CompareTo(other._rawValue);
        public override string ToString() => ToDouble().ToString();
        
        public static Fixed64 Abs(Fixed64 value) => new(System.Math.Abs(value._rawValue));
        public static Fixed64 Sqrt(Fixed64 value)
        {
            if (value._rawValue < 0) throw new ArgumentOutOfRangeException();
            if (value._rawValue == 0) return Zero;
            // 牛顿迭代法
            long x = value._rawValue;
            long result = x;
            while (true)
            {
                long next = (result + x / result) >> 1;
                if (next >= result) break;
                result = next;
            }
            return new Fixed64(result);
        }
        // 三角函数可先用查表法或近似多项式，此处省略
    }
}