using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.Inputs;
using Core.Math;
using Core.Net.Events;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.UI;
using Game.Vfx;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.Present;
using HotUpdate.Game.Race.View;
using HotUpdate.UI.Loading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HotUpdate.UI
{
    /// <summary>
    /// 一场比赛的上下文：封装逻辑世界 + 玩家/AI 表现 + HUD 的完整生命周期
    /// </summary>
    public class RaceContext
    {
        private readonly IEventCenter _eventCenter;
        private readonly ObjectSpawner _objectSpawner;
        private readonly NetGameManager _netGameManager;
        private readonly INetManager _netManager;
        private readonly IInputSystem _inputSystem;
        private readonly IUIManager _uiManager;

        private LogicWorld _logicWorld;
        private readonly List<ViewAvatar> _viewAvatars = new();
        // effectId → 资源 key
        private readonly Dictionary<int, string> _effectKeyMap = new()
        {
            {0, AssetKeys.Attack1},
            { 1, AssetKeys.AoeAttack },
        }; 
        // 场地视觉（地板/墙/障碍物），离场释放
        private readonly List<GameObject> _levelVisuals = new();
        
        public ERaceState State { get; private set; } = ERaceState.None;
        public LogicWorld World => _logicWorld;

        public RaceContext(IEventCenter eventCenter, ObjectSpawner objectSpawner, NetGameManager netGameManager, IInputSystem inputSystem, IUIManager uiManager, INetManager netManager)
        {
            _eventCenter = eventCenter;
            _objectSpawner = objectSpawner;
            _netGameManager = netGameManager;
            _netManager = netManager;
            _inputSystem = inputSystem;
            _uiManager = uiManager;
        }
        
        /// <summary>
        /// 构建这一局的逻辑世界 + 玩家/AI 表现 + HUD
        /// </summary>
        public async Task PrepareAsync(int selfRaceId, int[] raceIds)
        {
            State = ERaceState.Preparing;

            // 加载界面 + 游戏界面
            await _uiManager.CreateViewAsync<LoadingView, LoadingController>(AssetKeys.LoadingPanel, E_UILayer.Bot);
            var raceController = await _uiManager.CreateViewAsync<RaceView, RaceController>(AssetKeys.GameView, E_UILayer.Mid);

            // 逻辑世界
            _logicWorld = DIContainer.Create<LogicWorld>();
            ReplayRecorder.Start(raceIds);   // 开始录制本局

            // 关卡（确定性配置 + 程序化场地视觉）
            var level = LevelTable.Default;
            _logicWorld.SetLevel(level);
            await SpawnLevelVisuals(level);
            
            // 玩家（逻辑 + 表现 + HUD）
            for (var i = 0; i < raceIds.Length; i++)
            {
                var raceId = raceIds[i];
                var logicAvatar = new LogicAvatar(raceId, CharacterTable.Get(CharacterTable.PlayerId));
                logicAvatar.Spawn(level.PlayerSpawnPoints[i]);
                _logicWorld.AddAvatar(logicAvatar);

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.Role1);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);

                if (selfRaceId == raceId)
                {
                    var playerInput = viewAvatar.gameObject.AddComponent<PlayerInput>();
                    viewAvatar.InitInput(_inputSystem, playerInput);
                    _netGameManager.SetCurrentRaceId(selfRaceId);
                    raceController.SetCurrentAvatar(viewAvatar);
                }
                _netGameManager.AddPlayer(raceId, viewAvatar);

                // HUD 是 UI 显示，交给 RaceController（它内部用 view.transform 做父节点）
                await raceController.CreateHUD(viewAvatar);
            }

            // AI
            await SpawnAi(level);
            
            // 监听连接事件：比赛期间的同进程重连由这里处理（MainController 隐藏后不再监听）
            _netManager.OnConnected += OnConnected;
            // 监听比赛结束
            _eventCenter.SubscribeEvent<RaceEndEvent>(OnRaceEnd);
            // 监听特效播放
            _eventCenter.SubscribeEvent<PresentEventsEvent>(OnPresentEvents);
        }
        
        /// <summary>
        /// 比赛开始：销毁加载界面，切到 Playing
        /// </summary>
        public async Task StartRace()
        {
            State = ERaceState.Playing;
            var controller = _uiManager.GetController<LoadingController>();
            await _uiManager.DestroyView(controller.PanelId);
        }
        
        /// <summary>
        /// 重连：触发追帧流程
        /// </summary>
        public void Reconnect()
        {
            // 显示提示UI
            // 正在重新连接到到比赛...
            
            State = ERaceState.Reconnecting;
            var evt = EventSource.Get<RequestReconnectRaceEvent>();
            _eventCenter.TriggerEvent(evt);
        }
        
        /// <summary>
        /// 比赛期间连接事件：同进程重连（服务器仍有比赛）时触发追帧
        /// </summary>
        private void OnConnected(ConnectResult connectResult)
        {
            if (connectResult.RaceExist)
            {
                Reconnect();
            }
        }
        
        /// <summary>
        /// 离开比赛：退订逻辑世界，释放表现对象
        /// </summary>
        public void Leave()
        {
            if (State == ERaceState.Ended)
                return;
            
            State = ERaceState.Ended;
            _logicWorld?.Unsubscribe();
            _netManager.OnConnected -= OnConnected;
            _eventCenter.UnsubscribeEvent<RaceEndEvent>(OnRaceEnd);
            _eventCenter.UnsubscribeEvent<PresentEventsEvent>(OnPresentEvents);
            _objectSpawner.Release(_viewAvatars, true);
            _objectSpawner.Release(_levelVisuals, true);
        }
        
        /// <summary>
        /// 比赛结束：返回主界面并清理比赛
        /// </summary>
        private async void OnRaceEnd(RaceEndEvent evt)
        {
            if (State == ERaceState.Ended)
                return;
            
            // 通知服务器比赛结束，允许重新匹配
            _netManager.Send(new C2S_RaceEndMessage(), EProtocolChannel.Resolve);
            ReplayRecorder.Stop();   // 停止录制，保留供回放

            // 关闭战斗界面（RaceView，会顺带释放 HUD）
            var raceController = _uiManager.GetController<RaceController>();
            if (raceController != null)
            {
                await _uiManager.DestroyView(raceController.PanelId);
            }
            
            // 显示结果面板，点"返回大厅"才回主界面
            var resultPanel = await _objectSpawner.SpawnAsync<ResultPanelUI>(AssetKeys.ResultView, _uiManager.GetLayer(E_UILayer.Mid));
            resultPanel.Init(evt.Win, () =>
            {
                _objectSpawner.Release(resultPanel);
                ReturnToLobby();
            });
        }
        
        private async void ReturnToLobby()
        {
            // 返回主界面
            var mainController = _uiManager.GetController<MainController>();
            if (mainController != null)
                await _uiManager.SetViewActive(mainController.PanelId, true);

            // 清理比赛
            Leave();
        }
        
        private async void OnPresentEvents(PresentEventsEvent evt)
        {
            foreach (var e in evt.Events.ToArray())
            {
                if (e.Type != EPresentEventType.SpawnEffect)
                    continue;
                if (!_effectKeyMap.TryGetValue(e.Id, out var key) || string.IsNullOrEmpty(key))
                    continue;
                var rotation = e.Dir.SqrMagnitude() != Fixed64.Zero
                    ? Quaternion.LookRotation(e.Dir.ToVector3())
                    : Quaternion.identity;
                var timer = await _objectSpawner.SpawnAsync<VfxTimer>(key, null, e.Pos.ToVector3(), rotation);
                timer.overCallback += () =>
                {
                    _objectSpawner.Release(timer);
                };
            }
        }
        
        private async Task SpawnAi(LevelConfig level)
        {
            const int AiCount = 1;
            for (var i = 0; i < AiCount; i++)
            {
                var aiId = -1000 - i;
                var logicAvatar = new LogicAvatar(aiId, CharacterTable.Get(CharacterTable.MonsterId));
                logicAvatar.Spawn(level.MonsterSpawnPoints[i]);
                _logicWorld.AddAvatar(logicAvatar);

                var random = new DeterministicRandom((uint)(10000 + i));
                _logicWorld.AddAi(new AiController(logicAvatar));

                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.AIRole);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);
                
                // HUD 是 UI 显示，交给 RaceController（它内部用 view.transform 做父节点）
                await _uiManager.GetController<RaceController>().CreateHUD(viewAvatar);
            }
        }
        
        private async Task SpawnLevelVisuals(LevelConfig level)
        {
            float minX = level.BoundsMin.x.ToFloat(), maxX = level.BoundsMax.x.ToFloat();
            float minZ = level.BoundsMin.z.ToFloat(), maxZ = level.BoundsMax.z.ToFloat();
            float cx = (minX + maxX) / 2f, cz = (minZ + maxZ) / 2f;
            float lenX = maxX - minX, lenZ = maxZ - minZ;

            // 地板（按实际 mesh 尺寸缩放铺满边界；若场景已有地面可省）
            var floor = await _objectSpawner.SpawnAsync<GameObject>(AssetKeys.Floor, null, new Vector3(cx, 0f, cz), Quaternion.identity);
            var floorSize = GetMeshSize(floor);
            floor.transform.localScale = new Vector3(lenX / floorSize.x, 1f, lenZ / floorSize.z);
            // 地面 pivot 在中心，下移半个厚度让上表面贴到 Y=0（人物脚底地面），否则人物会埋进地面
            floor.transform.position = new Vector3(cx, -floorSize.y / 2f, cz);
            _levelVisuals.Add(floor);

            // 4面墙（单位立方体，pivot 中心，边长 1；sx/sz 是沿世界 X/Z 的缩放）
            const float thickness = 0.5f;
            await SpawnWall(cx, minZ - thickness / 2f, lenX + 2f * thickness, thickness);  // 底边（沿 X 长）
            await SpawnWall(cx, maxZ + thickness / 2f, lenX + 2f * thickness, thickness);  // 顶边（沿 X 长）
            await SpawnWall(minX - thickness / 2f, cz, thickness, lenZ + 2f * thickness);  // 左边（沿 Z 长）
            await SpawnWall(maxX + thickness / 2f, cz, thickness, lenZ + 2f * thickness);  // 右边（沿 Z 长）

            // 障碍物（prefab 自然直径约 1，scale = 2 * radius）
            foreach (var ob in level.Obstacles)
            {
                var obj = await _objectSpawner.SpawnAsync<GameObject>(AssetKeys.Obstacle, null, ob.Center.ToVector3(), Quaternion.identity);
                var d = (ob.Radius * Fixed64.FromInt(2)).ToFloat();
                obj.transform.localScale = new Vector3(d, d, d);
                _levelVisuals.Add(obj);
            }
        }

        private async Task SpawnWall(float x, float z, float sx, float sz)
        {
            const float height = 1.5f;   // 墙高，世界单位
            var wall = await _objectSpawner.SpawnAsync<GameObject>(AssetKeys.Wall, null, new Vector3(x, height / 2f, z), Quaternion.identity);
            var wallSize = GetMeshSize(wall);
            wall.transform.localScale = new Vector3(sx / wallSize.x, height / wallSize.y, sz / wallSize.z);
            _levelVisuals.Add(wall);
        }
        
        /// <summary>
        /// 读 prefab 实际 mesh 尺寸（local 未缩放），避免假设单位尺寸
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static Vector3 GetMeshSize(GameObject obj)
        {
            var mf = obj.GetComponentInChildren<MeshFilter>();
            if (mf && mf.sharedMesh)
                return mf.sharedMesh.bounds.size;
            return Vector3.one;
        }
    }
}
