using System.Threading.Tasks;
using Core.DI;
using Core.GlobalEvent;
using Core.GlobalEvent.Events;
using Core.Log;
using Core.Mono;
using Core.Serialize.Json;
using Core.Singleton;
using Core.Utility;

namespace Core.Global
{
    /// <summary>
    /// 游戏设置管理器
    /// </summary>
    public class GameSettingManager : IGameSettingManager, IApplicationExitNotify, IInitializable
    {
        public int InitPriority => 1;
        public int QuitPriority => 0;
        
        [Inject] private IJsonManager _jsonManager;
        [Inject] private IMonoAdapter _monoAdapter;
        [Inject] private IEventCenter _eventCenter;

        // 游戏设置
        public GameSetting GameSetting { get; private set; }
        
        private GameSettingManager(){}
        
        public async Task InitAsync()
        {
            GameSetting = await _jsonManager.FromJsonAsync<GameSetting>($"{PathUtility.GetUserDataLocalSavePath(FileUtility.GameSettingFileName)}");
            GameSetting.enableTypewriter = true;
        }
        
        /// <summary>
        /// 设置启用打印机效果
        /// </summary>
        /// <param name="enable"></param>
        public void SetEnableTypewriter(bool enable)
        {
            GameSetting.enableTypewriter = enable;
            _eventCenter.TriggerEvent(new GameSettingUpdateEvent {GameSetting = GameSetting});
        }
        
        public void OnAppQuit()
        {
            _jsonManager.SaveToJson(GameSetting, $"{PathUtility.GetUserDataLocalSavePath(FileUtility.GameSettingFileName)}");
            Logger.Log($"{nameof(GameSettingManager)}.{nameof(OnAppQuit)}:游戏设置数据保存成功，{GameSetting}");
        }
    }
}
