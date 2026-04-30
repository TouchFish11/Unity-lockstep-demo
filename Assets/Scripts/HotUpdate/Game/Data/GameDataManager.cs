using System.Threading.Tasks;
using Core.DI;
using Core.Mono;
using HotUpdate.Base.Items;

namespace HotUpdate.Game.Data
{
    /// <summary>
    /// 游戏数据管理器
    /// </summary>
    public class GameDataManager : IApplicationExitNotify
    {
        public int QuitPriority => 2;
        
        public ItemDataProvider ItemDataProvider { get; }
        
        public GameDataManager(IMonoAdapter monoAdapter)
        {
            monoAdapter.AddApplicationExitNotify(this);
            ItemDataProvider = DIContainer.Create<ItemDataProvider>();
        }
        
        /// <summary>
        /// 加载所有配置
        /// </summary>
        public async Task LoadConfigAsync()
        {
            await ItemDataProvider.LoadConfigAsync();
            // ...
        }
        
        /// <summary>
        /// 加载所有玩家数据
        /// </summary>
        public async Task LoadDataAsync()
        {
            await ItemDataProvider.LoadDataAsync();
        }

        public void SaveDatas()
        {
            ItemDataProvider.SaveData();
        }
        
        public void OnAppQuit()
        {
            SaveDatas();
        }
    }
}
