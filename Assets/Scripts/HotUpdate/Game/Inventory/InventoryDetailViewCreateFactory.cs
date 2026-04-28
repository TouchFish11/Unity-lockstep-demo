using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Pool;
using HotUpdate.Common.Config.Item;
using HotUpdate.Game.Inventory.UI.Detail;
using UnityEngine;

namespace HotUpdate.Game.Inventory
{
    public class InventoryDetailViewCreateFactory : IPoolData
    {
        [Inject] private ObjectSpawner _objectSpawner;
        
        public async Task<PoolObject> CreateDetailPanel(EItemType itemType, RectTransform detailArea)
        {
            switch (itemType)
            {
                case EItemType.Material:
                    var poolObject = await _objectSpawner.SpawnAsync<MaterialDetailPanel>(AssetKeys.MaterialDetailPanel, detailArea);
                    poolObject.Obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    return poolObject;
                case EItemType.Weapon:
                    await _objectSpawner.SpawnAsync<MaterialDetailPanel>("Weapon", detailArea);
                    return default;
                case EItemType.HolyRelic:
                    await _objectSpawner.SpawnAsync<MaterialDetailPanel>("HolyRelic", detailArea);
                    return default;
                case EItemType.precious:
                    await _objectSpawner.SpawnAsync<MaterialDetailPanel>("precious", detailArea);
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
