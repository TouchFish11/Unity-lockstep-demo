using System.Collections.Generic;
using Core.Net.Protocols.FSync;

namespace HotUpdate.Game.Race.Logic
{
    public static class ReplayRecorder
    {
        public static bool IsRecording { get; private set; }
        public static int[] PlayerRaceIds { get; private set; }
        public static List<List<InputCommand>> Frames { get; } = new();

        public static void Start(int[] raceIds)
        {
            PlayerRaceIds = raceIds;
            Frames.Clear();
            IsRecording = true;
        }

        public static void Stop()
        {
            IsRecording = false;
        }

        public static void Record(List<InputCommand> commands)
        {
            if (!IsRecording)
                return;
            
            Frames.Add(new List<InputCommand>(commands)); // 快照拷贝
        }

        public static bool HasRecording()
        {
            return PlayerRaceIds != null && Frames.Count > 0;
        }
    }
}