using System;
using UnityEngine;

namespace Core.Math
{
    public struct FixedVector3 : IEquatable<FixedVector3>
    {
        public Fixed64 x, y, z;

        public FixedVector3(Fixed64 x, Fixed64 y, Fixed64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static FixedVector3 Zero => new FixedVector3(Fixed64.Zero, Fixed64.Zero, Fixed64.Zero);
        public static FixedVector3 One => new FixedVector3(Fixed64.One, Fixed64.One, Fixed64.One);

        // 加减
        public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b) => new FixedVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static FixedVector3 operator -(FixedVector3 a, FixedVector3 b) => new FixedVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        // 缩放
        public static FixedVector3 operator *(FixedVector3 a, Fixed64 scalar) => new FixedVector3(a.x * scalar, a.y * scalar, a.z * scalar);
        public static FixedVector3 operator /(FixedVector3 a, Fixed64 scalar) => new FixedVector3(a.x / scalar, a.y / scalar, a.z / scalar);

        public static bool operator ==(FixedVector3 a, FixedVector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;

        public static bool operator !=(FixedVector3 a, FixedVector3 b) => !(a == b);

        // 点积
        public Fixed64 Dot(FixedVector3 other) => x * other.x + y * other.y + z * other.z;

        // 平方长度
        public Fixed64 SqrMagnitude() => Dot(this);

        // 长度
        public Fixed64 Magnitude() => Fixed64.Sqrt(SqrMagnitude());

        // 归一化
        public FixedVector3 Normalized()
        {
            var mag = Magnitude();
            if (mag == Fixed64.Zero) 
                return Zero;
            return this / mag;
        }

        // 叉积
        public static FixedVector3 Cross(FixedVector3 a, FixedVector3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        // 距离
        public static Fixed64 Distance(FixedVector3 a, FixedVector3 b) => (a - b).Magnitude();

        // 转换为 Unity Vector3（仅渲染用）
        public Vector3 ToVector3() => new(x.ToFloat(), y.ToFloat(), z.ToFloat());

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public bool Equals(FixedVector3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedVector3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }
}