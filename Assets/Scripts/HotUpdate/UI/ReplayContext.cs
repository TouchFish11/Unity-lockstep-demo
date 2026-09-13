using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.GlobalEvent;
using Core.Math;
using Core.Mono;
using Core.Time;
using Core.UI;
using Game.Vfx;
using HotUpdate.Game.Race.Logic;
using HotUpdate.Game.Race.Present;
using HotUpdate.Game.Race.View;
using UnityEngine;

namespace HotUpdate.UI
{
    public class ReplayContext
    {
        private readonly IEventCenter _eventCenter;
        private readonly ObjectSpawner _objectSpawner;
        private readonly IMonoAdapter _monoAdapter;
        private readonly IUIManager _uiManager;
        private readonly Dictionary<int, string> _effectKeyMap = new()
        {
            {0, AssetKeys.Attack1},
            { 1, AssetKeys.AoeAttack },
        }; 
        private readonly List<GameObject> _levelVisuals = new();

        private LogicWorld _logicWorld;
        private readonly List<ViewAvatar> _viewAvatars = new();
        private readonly List<StatusHUD> _huds = new();
        private int _frameIndex;
        private float _accumulator;
        private const float LogicTime = 0.066f;

        public ReplayContext(IEventCenter eventCenter, ObjectSpawner objectSpawner, IMonoAdapter monoAdapter, IUIManager uiManager)
        {
            _eventCenter = eventCenter;
            _objectSpawner = objectSpawner;
            _monoAdapter = monoAdapter;
            _uiManager = uiManager;
        }

        public async Task PrepareAsync()
        {
            _logicWorld = DIContainer.Create<LogicWorld>();
            var level = LevelTable.Default; 
            _logicWorld.SetLevel(level); 
            await SpawnLevelVisuals(level);
            
            _eventCenter.SubscribeEvent<PresentEventsEvent>(OnPresentEvents);

            for (var i = 0; i < ReplayRecorder.PlayerRaceIds.Length; i++)
            {
                var raceId = ReplayRecorder.PlayerRaceIds[i];
                var logicAvatar = new LogicAvatar(raceId, CharacterTable.Get(CharacterTable.PlayerId));
                logicAvatar.Spawn(level.PlayerSpawnPoints[i]);
                _logicWorld.AddAvatar(logicAvatar);
                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.Role1);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);
                await SpawnHud(viewAvatar);
            }

            await SpawnAi(level);

            _monoAdapter.AddUpdateListener(OnUpdate);
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
                _logicWorld.AddAi(new AiController(logicAvatar));
                
                using var handle = await GameAsset.LoadAssetAsync<RuntimeAnimatorController>(AssetKeys.Role1_Animator);
                var viewAvatar = await _objectSpawner.SpawnAsync<ViewAvatar>(AssetKeys.AIRole);
                viewAvatar.Bind(logicAvatar, handle.Asset);
                _viewAvatars.Add(viewAvatar);
                await SpawnHud(viewAvatar);
            }
        }

        private async Task SpawnHud(ViewAvatar viewAvatar)
        {
            var layer = _uiManager.GetLayer(E_UILayer.Mid);
            var hud = await _objectSpawner.SpawnAsync<StatusHUD>(AssetKeys.StatusHUD, layer);
            hud.Init(viewAvatar, layer);
            _huds.Add(hud);
        }

        private void OnUpdate()
        {
            _accumulator += TimeUtil.DeltaTime;
            if (_accumulator < LogicTime)
                return;
            _accumulator -= LogicTime;

            if (_frameIndex >= ReplayRecorder.Frames.Count)
            {
                ExitReplay();
                return;
            }
            _logicWorld.Tick(ReplayRecorder.Frames[_frameIndex++]);
        }

        private async void OnPresentEvents(PresentEventsEvent evt)
        {
            var events = new List<PresentEvent>(evt.Events); // 快照，隔离池化列表
            foreach (var e in events)
            {
                if (e.Type != EPresentEventType.SpawnEffect)
                    continue;
                if (!_effectKeyMap.TryGetValue(e.Id, out var key) || string.IsNullOrEmpty(key))
                    continue;
                var timer = await _objectSpawner.SpawnAsync<VfxTimer>(key, null, e.Pos.ToVector3(), Quaternion.identity);
                timer.overCallback += () =>
                {
                    _objectSpawner.Release(timer);
                };
            }
        }

        private async void ExitReplay()
        {
            _monoAdapter.RemoveUpdateListener(OnUpdate);
            _eventCenter.UnsubscribeEvent<PresentEventsEvent>(OnPresentEvents);
            _logicWorld?.Unsubscribe();
            _objectSpawner.Release(_huds);
            _objectSpawner.Release(_viewAvatars, true);
            _objectSpawner.Release(_levelVisuals, true);
            var mainController = _uiManager.GetController<MainController>();
            if (mainController != null)
                await _uiManager.SetViewActive(mainController.PanelId, true);
        }
        
        private async Task SpawnLevelVisuals(LevelConfig level)
        {
            float minX = level.BoundsMin.x.ToFloat(), maxX = level.BoundsMax.x.ToFloat();
            float minZ = level.BoundsMin.z.ToFloat(), maxZ = level.BoundsMax.z.ToFloat();
            float cx = (minX + maxX) / 2f, cz = (minZ + maxZ) / 2f;
            float lenX = maxX - minX, lenZ = maxZ - minZ;

            // 地板（单位平面，缩放铺满边界；若场景已有地面可省）
            var floor = await _objectSpawner.SpawnAsync<GameObject>(AssetKeys.Floor, null, new Vector3(cx, 0f, cz), Quaternion.identity);
            floor.transform.localScale = new Vector3(lenX, 1f, lenZ);
            _levelVisuals.Add(floor);

            // 面墙（单位立方体，pivot 中心，边长 1；sx/sz 是沿世界 X/Z 的缩放）
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
            wall.transform.localScale = new Vector3(sx, height, sz);
            _levelVisuals.Add(wall);
        }
    }
}