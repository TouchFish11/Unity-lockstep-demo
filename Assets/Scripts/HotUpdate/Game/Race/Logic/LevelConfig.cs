using System.Collections.Generic;
using Core.Math;

namespace HotUpdate.Game.Race.Logic
{
    public class LevelObstacle
    {
        public FixedVector3 Center;
        public Fixed64 Radius;
    }
    
    public class LevelConfig
    {
        public FixedVector3 BoundsMin;               // 左下角（x,z 有意义，y 恒 0）
        public FixedVector3 BoundsMax;               // 右上角
        public List<LevelObstacle> Obstacles;        // 圆形障碍物
        public List<FixedVector3> PlayerSpawnPoints; // 玩家出生点，按 raceIds 顺序取
        public List<FixedVector3> MonsterSpawnPoints;// 怪物出生点
    }

    public static class LevelTable
    {
        public static readonly LevelConfig Default = BuildDefault();

        private static LevelConfig BuildDefault()
        {
            return new LevelConfig
            {
                BoundsMin = new FixedVector3(Fixed64.FromInt(-8), Fixed64.Zero, Fixed64.FromInt(-8)),
                BoundsMax = new FixedVector3(Fixed64.FromInt(8),  Fixed64.Zero, Fixed64.FromInt(8)),
                Obstacles = new List<LevelObstacle>
                {
                    new() { Center = new FixedVector3(Fixed64.Zero, Fixed64.Zero, Fixed64.Zero), Radius = Fixed64.FromFloat(1.2f) },
                    new() { Center = new FixedVector3(Fixed64.FromInt(4), Fixed64.Zero, Fixed64.FromInt(4)), Radius = Fixed64.FromFloat(0.8f) },
                    new() { Center = new FixedVector3(Fixed64.FromInt(-4), Fixed64.Zero, Fixed64.FromInt(-4)), Radius = Fixed64.FromFloat(0.8f) },
                },
                PlayerSpawnPoints = new List<FixedVector3>
                {
                    new(Fixed64.FromInt(-6), Fixed64.Zero, Fixed64.FromInt(-6)),
                    new(Fixed64.FromInt(6),  Fixed64.Zero, Fixed64.FromInt(6)),
                },
                MonsterSpawnPoints = new List<FixedVector3>
                {
                    new(Fixed64.FromInt(6), Fixed64.Zero, Fixed64.FromInt(-6)),
                },
            };
        }
    }
}