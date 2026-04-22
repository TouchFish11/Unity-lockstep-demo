using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.Mono;
using Core.Serialize.Json;
using Core.Utility;
using HotUpdate.Common.Config.Item;
using HotUpdate.Common.Data.Inventory;
using HotUpdate.Common.Utility;
using UnityEngine;

namespace HotUpdate.Game.Data
{
    /// <summary>
    /// 游戏数据管理器
    /// </summary>
    public class GameDataManager : IApplicationExitNotify
    {
        public int QuitPriority => 2;
        
        private readonly IJsonManager _jsonManager;
        
        public ItemDataCollection ItemDataCollection { get; private set; }
        
        public ItemConfigCollection ItemConfigCollection { get; private set; }

        public GameDataManager(IMonoAdapter monoAdapter, IJsonManager jsonManager)
        {
            monoAdapter.AddApplicationExitNotify(this);
            _jsonManager = jsonManager;
        }

        public async Task LoadConfigAsync()
        {
            var handle = await GameAsset.LoadAssetAsync<TextAsset>(AssetKeys.Itemconfigs);
            ItemConfigCollection = _jsonManager.FromJson<ItemConfigCollection>(handle.Asset.text, settings: NewtonsoftJsonUtility.SerializerSettings);
            GameAsset.Release(handle);
        }
        
        /// <summary>
        /// 加载玩家数据和配置数据
        /// </summary>
        public async Task LoadDataAsync()
        {
            ItemDataCollection = await _jsonManager.FromJsonAsync<ItemDataCollection>(PathUtility.GetUserDataLocalSavePath(GameFileUtil.PlayerItemDataFileName), settings: NewtonsoftJsonUtility.SerializerSettings);
        }

        public void SaveData()
        {
            _jsonManager.SaveToJson(ItemDataCollection, PathUtility.GetUserDataLocalSavePath(GameFileUtil.PlayerItemDataFileName), settings: NewtonsoftJsonUtility.SerializerSettings);
        }
        
        public void OnAppQuit()
        {
            SaveData();
        }
    }
}
