using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Pool;
using HotUpdate.Common.Items;
using HotUpdate.Game.Inventory.UI.Detail;
using UnityEngine;

namespace HotUpdate.Game.Inventory
{
    /// <summary>
    /// 背包详细界面创建工厂
    /// </summary>
    public class InventoryDetailViewCreateFactory : IPoolData
    {
        [Inject] private ObjectSpawner _objectSpawner;
        
        public async Task<PoolObject> CreateDetailPanel(EItemType itemType, RectTransform detailArea)
        {
            switch (itemType)
            {
                case EItemType.Material:
                    var MaterialObject = await _objectSpawner.SpawnAsync<MaterialDetailPanel>(AssetKeys.MaterialDetailPanel, detailArea, Vector2.zero);
                    return MaterialObject;
                case EItemType.Weapon:
                    var WeaponObject = await _objectSpawner.SpawnAsync<WeaponDetailPanel>(AssetKeys.WeaponDetailPanel, detailArea, Vector2.zero);
                    return WeaponObject;
                case EItemType.HolyRelic:
                    // TODO：await _objectSpawner.SpawnAsync<MaterialDetailPanel>("HolyRelic", detailArea);
                    return default;
                case EItemType.precious:
                    // TODO：await _objectSpawner.SpawnAsync<MaterialDetailPanel>("precious", detailArea);
                    return default;
                default:
                    return default;
            }
        }
        
        void IPoolData.ResetData()
        {
            _objectSpawner.Dispose();
            _objectSpawner = null;
        }
    }
}
