using System;
using Core.Math;
using Core.Net.Protocols.FSync.Messages;

namespace Core.Net.Protocols.FSync
{
    /// <summary>
    /// 命令编解码器
    /// </summary>
    public static class CommandCodec
    {
        public static OptMessage Encode(int playerId, in InputCommand inputCommand)
        {
            var optMessage = new OptMessage
            {
                RaceID = playerId,
                OptType = (byte)inputCommand.optType
            };
            
            switch (inputCommand.optType)
            {
                case EOptType.None:
                    break;
                case EOptType.Move:
                    optMessage.Arg1 = (int)inputCommand.dir.x.RawValue;
                    optMessage.Arg2 = (int)inputCommand.dir.y.RawValue;
                    optMessage.Arg3 = (int)inputCommand.dir.z.RawValue;
                    break;
                case EOptType.Attack:
                    optMessage.Arg1 = inputCommand.targetId;
                    break;
                case EOptType.UseSkill:
                    optMessage.Arg1 = inputCommand.skillId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return optMessage;
        }

        public static InputCommand Decode(OptMessage optMessage)
        {
            var inputCommand = new InputCommand
            {
                raceId = optMessage.RaceID
            };
            switch ((EOptType)optMessage.OptType)
            {
                case EOptType.None:
                    inputCommand.optType = EOptType.None;
                    break;
                case EOptType.Move:
                    inputCommand.optType = EOptType.Move;
                    inputCommand.dir = new FixedVector3(Fixed64.FromRaw(optMessage.Arg1), Fixed64.FromRaw(optMessage.Arg2), Fixed64.FromRaw(optMessage.Arg3));
                    break;
                case EOptType.Attack:
                    inputCommand.optType = EOptType.Attack;
                    inputCommand.targetId = optMessage.Arg1;
                    break;
                case EOptType.UseSkill:
                    inputCommand.optType = EOptType.UseSkill;
                    inputCommand.skillId = optMessage.Arg1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return inputCommand;
        }
    }
}
