using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Core.Mono;
using Core.Net.SyncModule.Interface;
using Core.Serialize.Json;
using Core.Utility;

namespace Core.Net.SyncModule.Manager
{
    public class NetGameManager : IApplicationExitNotify
    {
        private readonly IJsonManager _jsonManager;
        private readonly Dictionary<int, INetObject> _idToPlayerMap = new();
        private int? _raceId;

        /// <summary>
        /// 当前客户端的比赛ID
        /// </summary>
        public int? RaceId => _raceId;

        private NetGameManager(IJsonManager jsonManager, IMonoAdapter monoAdapter)
        {
            monoAdapter.AddApplicationExitNotify(this);
            _jsonManager = jsonManager;
            var path = PathUtility.GetUserDataLocalSavePath("ReconnectGameTestData.json");
            if (File.Exists(path))
            {
                if(int.TryParse(_jsonManager.Load(path), NumberStyles.Integer, CultureInfo.InvariantCulture, out var raceId))
                {
                    _raceId = raceId;
                }
            }
        }

        public bool TryGetPlayer(int clientId, out INetObject netObject)
        {
            return _idToPlayerMap.TryGetValue(clientId, out netObject);
        }

        public void AddPlayer(int clientId, INetObject netObject)
        {
            _idToPlayerMap.Add(clientId, netObject);
        }

        public void SetCurrentRaceId(int raceId)
        {
            _raceId = raceId;
        }

        public void ClearLocalCache()
        {
            _raceId = null;
            var path = PathUtility.GetUserDataLocalSavePath("ReconnectGameTestData.json");
            _jsonManager.Save($"{_raceId}", path);
        }
        
        public int QuitPriority => 1;
        
        public void OnAppQuit()
        {
            var path = PathUtility.GetUserDataLocalSavePath("ReconnectGameTestData.json");
            _jsonManager.SaveToJson(_raceId, path);
        }
    }
}
