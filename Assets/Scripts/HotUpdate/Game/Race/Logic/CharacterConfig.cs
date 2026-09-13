using System.Collections.Generic;
using Core.Math;

namespace HotUpdate.Game.Race.Logic
{
    public class CharacterConfig
    {
        public int id;
        public string name;
        public int maxHp;
        public Fixed64 speed;
        public Fixed64 radius;
        public bool isPlayer;
    }

    public static class CharacterTable
    {
        public const int PlayerId = 0;    // 配置 id，区别于 LogicAvatar.PlayerId（比赛身份）
        public const int MonsterId = 1;

        private static readonly CharacterConfig Player = new()
        {
            id = PlayerId, 
            name = "玩家", 
            maxHp = 100,
            speed = Fixed64.FromFloat(3f), 
            radius = Fixed64.FromFloat(0.5f), 
            isPlayer = true,
        };

        private static readonly CharacterConfig Monster = new()
        {
            id = MonsterId, 
            name = "怪物", 
            maxHp = 150,
            speed = Fixed64.FromFloat(2f), 
            radius = Fixed64.FromFloat(0.5f), 
            isPlayer = false,
        };

        private static readonly Dictionary<int, CharacterConfig> Map = new()
        {
            { PlayerId, Player },
            { MonsterId, Monster },
        };

        public static CharacterConfig Get(int id) => Map.GetValueOrDefault(id);
    }
}