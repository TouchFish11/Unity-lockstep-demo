using System.Collections.Generic;
using Core.Math;

namespace HotUpdate.Game.Race.Logic
{
    public enum EAbilityShape : byte
    {
        Fan = 0,     // 扇形（普攻）
        Circle = 1,  // 自身圆形范围（AoE）
    }

    public class AbilityConfig
    {
        public int Id;
        public string Name;
        public EAbilityShape Shape;
        public Fixed64 Range;          // Fan=圆心距离，Circle=半径
        public Fixed64 HalfAngleCos;   // 仅 Fan 用
        public int Damage;
        public int Duration;           // 逻辑帧
        public int HitFrame;           // 第几帧结算伤害
        public int Cooldown;           // 冷却帧，0=无
        public int EffectId;           // 0=无特效
        public ELogicAnimState AnimState;
    }

    public static class AbilityTable
    {
        public const int AttackId = 0;
        public const int AoeSkillId = 1;

        private static readonly AbilityConfig Attack = new()
        {
            Id = AttackId, 
            Name = "普攻", 
            Shape = EAbilityShape.Fan,
            Range = Fixed64.FromFloat(2f), 
            HalfAngleCos = Fixed64.Half,
            Damage = 0, 
            Duration = 8, 
            HitFrame = 3, 
            Cooldown = 0,
            EffectId = 0, 
            AnimState = ELogicAnimState.Attack,
        };

        private static readonly AbilityConfig AoeSkill = new()
        {
            Id = AoeSkillId, 
            Name = "回旋斩", 
            Shape = EAbilityShape.Circle,
            Range = Fixed64.FromFloat(3f),
            Damage = 20, 
            Duration = 8, 
            HitFrame = 3, 
            Cooldown = 30,
            EffectId = 1, 
            AnimState = ELogicAnimState.Skill,
        };

        private static readonly Dictionary<int, AbilityConfig> Map = new()
        {
            { AttackId, Attack },
            { AoeSkillId, AoeSkill },
        };

        public static AbilityConfig Get(int id) => Map.GetValueOrDefault(id);
    }
}