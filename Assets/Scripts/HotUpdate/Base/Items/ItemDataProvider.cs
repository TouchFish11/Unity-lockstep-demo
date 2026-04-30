using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Serialize.Json;
using Core.Utility;
using HotUpdate.Base.Factory;
using HotUpdate.Common.Items;
using HotUpdate.Common.Items.Config;
using HotUpdate.Common.Items.Data;
using HotUpdate.Common.Utility;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Base.Items
{
    /// <summary>
    /// 玩家物品数据提供器
    /// </summary>
    public class ItemDataProvider
    {
        [Inject] private readonly IJsonManager _jsonManager;
        private ItemPersistentIdGenerator _idGenerator;
        
        private ItemDataCollection _itemDataCollection;
        // 用于可堆叠物品：itemId -> list索引
        private readonly Dictionary<int, int> _stackIndexByItemId =  new();
        // 用于不可堆叠物品：instanceId -> list索引
        private readonly Dictionary<long, int> _indexByInstanceId =  new();
        
        /// <summary>
        /// 物品配置全局缓存
        /// </summary>
        public Dictionary<int, ItemConfig> ConfigMap { get; } = new();
        
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="deltaNum">可堆叠则为初始数量；不可堆叠则为创建数量</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public void AddData(int itemId, int deltaNum)
        {
            // 获取该ID的物品配置
            var itemConfig = ConfigMap.GetValueOrDefault(itemId);
            if(itemConfig == null)
                throw new KeyNotFoundException($"[{nameof(ItemDataProvider)}]: Item {itemId} id not found");

            if (itemConfig.isPile)
            {
                // 可堆叠：查找已有数据
                if (TryGetData(itemId, out var exist))
                {
                    exist.itemNum += deltaNum;
                    // 注意：如果超出最大堆叠数，需要处理，这里先简化为直接加
                }
                else
                {
                    // 可堆叠物品持久化ID为默认ID
                    var newData = ItemDataCreateFactory.CreateData(itemConfig, deltaNum, ItemPersistentIdGenerator.DefaultNotStackableId);
                    // 新增玩家物品数据
                    _itemDataCollection.items.Add(newData);
                    // 同步缓存
                    _stackIndexByItemId.Add(itemId, _itemDataCollection.items.Count - 1);
                    Logger.Log($"{nameof(ItemDataCollection)}: Item(id = {itemId}, num = {deltaNum}) added");
                }
            }
            else
            {
                // 不可堆叠：每个都应该创建新条目
                for (var i = 0; i < deltaNum; i++)
                {
                    var newData = ItemDataCreateFactory.CreateData(itemConfig, 1, _idGenerator.AllocateId());
                    // 新增玩家物品数据
                    _itemDataCollection.items.Add(newData);
                    // 同步缓存
                    _indexByInstanceId.Add(newData.persistentId, _itemDataCollection.items.Count - 1);
                }
                Logger.Log($"{nameof(ItemDataCollection)}: Item(id = {itemId}, num = {deltaNum}) added");
            }
        }
        
        /// <summary>
        /// 移除物品数据
        /// </summary>
        /// <param name="id">可堆叠物品则为物品ID，不可堆叠物品则为实例ID</param>
        /// <param name="deltaNum">移除数量</param>
        public void RemoveData(int id, int deltaNum)
        {
            if(!ConfigMap.TryGetValue(id, out var config))
                throw new KeyNotFoundException($"[{nameof(ItemDataProvider)}]: Item {id} id not found");

            // 可堆叠物品的删除逻辑
            if (config.isPile)
            {
                // 找到要移除的物品数据在字典中的索引
                if(!_stackIndexByItemId.TryGetValue(id, out var index))
                    return;
                
                // 找到要删除的数据
                var removedData = _itemDataCollection.items[index];
                // 移除对应的数量
                removedData.itemNum -= deltaNum;
                if (removedData.itemNum > 0)
                    return;
                
                // 数量为0，完全移除该物品数据
                var lastIndex = _itemDataCollection.items.Count - 1;
                if (index != lastIndex)
                {
                    // 找到物品数据列表的末尾数据
                    var lastItem = _itemDataCollection.items[lastIndex];
                    // 将末尾数据覆盖要删除的数据
                    _itemDataCollection.items[index] = lastItem;
                    // 字典更新被移动元素的索引
                    _stackIndexByItemId[lastItem.itemId] = index;
                }

                // 移除末尾数据
                _itemDataCollection.items.RemoveAt(lastIndex);
                // 字典删除被移除元素的索引
                _stackIndexByItemId.Remove(removedData.itemId);
            }
            else
            {
                // 找到要移除的物品数据在字典中的索引
                if(!_indexByInstanceId.TryGetValue(id, out var index))
                    return;
                
                // 找到要删除的数据
                var removedData = _itemDataCollection.items[index];
                // 数量为0，完全移除该物品数据
                var lastIndex = _itemDataCollection.items.Count - 1;
                if (index != lastIndex)
                {
                    // 找到物品数据列表的末尾数据
                    var lastItem = _itemDataCollection.items[lastIndex];
                    // 将末尾数据覆盖要删除的数据
                    _itemDataCollection.items[index] = lastItem;
                    // 字典更新被移动元素的索引
                    _indexByInstanceId[lastItem.persistentId] = index;
                }

                // 移除末尾数据
                _itemDataCollection.items.RemoveAt(lastIndex);
                // 字典删除被移除元素的索引
                _indexByInstanceId.Remove(removedData.persistentId);
            }
        }
        
        /// <summary>
        /// 尝试获取可堆叠物品数据
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public bool TryGetData(int itemId, out ItemData itemData)
        {
            if (_stackIndexByItemId.TryGetValue(itemId, out var index))
            {
                if (index >= 0 && index < _itemDataCollection.items.Count)
                {
                    itemData = _itemDataCollection.items[index];
                    return true;
                }
            }
            itemData = null;
            return false;
        }
                
        /// <summary>
        /// 尝试获取不可堆叠物品数据
        /// </summary>
        /// <param name="persistentId"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public bool TryGetInstanceData(long persistentId, out ItemData itemData)
        {
            if (_indexByInstanceId.TryGetValue(persistentId, out var index))
            {
                if (index >= 0 && index < _itemDataCollection.items.Count)
                {
                    itemData = _itemDataCollection.items[index];
                    return true;
                }
            }
            itemData = null;
            return false;
        }

        /// <summary>
        /// 加载物品配置
        /// </summary>
        public async Task LoadConfigAsync()
        {
            using var handle = await GameAsset.LoadAssetAsync<TextAsset>(AssetKeys.ItemConfigs);
            var itemConfigCollection = _jsonManager.FromJson<ItemConfigCollection>(handle.Asset.text, settings: NewtonsoftJsonUtility.SerializerSettings);
            // 转存配置
            foreach (var itemConfig in itemConfigCollection.itemConfigs)
            {
                ConfigMap.Add(itemConfig.itemId, itemConfig);
            }
        }
        
        /// <summary>
        /// 加载玩家数据，依赖配置加载完成
        /// </summary>
        public async Task LoadDataAsync()
        {
            _itemDataCollection = await _jsonManager.FromJsonAsync<ItemDataCollection>(PathUtility.GetUserDataLocalSavePath(GameFileUtil.PlayerItemDataFileName), settings: NewtonsoftJsonUtility.SerializerSettings);
            
            // 初始化ID生成器
            _idGenerator = DIContainer.Create<ItemPersistentIdGenerator>(parameterValues: _itemDataCollection.nextPersistentId);
            
            // 重建字典缓存
            for (var i = 0; i < _itemDataCollection.items.Count; i++)
            {
                var itemData = _itemDataCollection.items[i];
                if (!ConfigMap.TryGetValue(itemData.itemId, out var config))
                    throw new KeyNotFoundException($"[{nameof(ItemDataProvider)}]: Item {itemData.itemId} not found");

                if (config.isPile)
                {
                    _stackIndexByItemId.Add(itemData.itemId, i);
                }
                else
                {
                    _indexByInstanceId.Add(itemData.persistentId, i);
                }
            }
        }
        
        /// <summary>
        /// 保存玩家数据
        /// </summary>
        public void SaveData()
        {
            _itemDataCollection.nextPersistentId = _idGenerator.CurrentMaxId + 1;
            _jsonManager.SaveToJson(_itemDataCollection, PathUtility.GetUserDataLocalSavePath(GameFileUtil.PlayerItemDataFileName), settings: NewtonsoftJsonUtility.SerializerSettings);
        }

        /// <summary>
        /// 通过物品类型获取所有的物品数据
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public IEnumerable<ItemData> GetItemsByType(EItemType itemType)
        {
            foreach (var itemData in _itemDataCollection.items)
            {
                if (ConfigMap.TryGetValue(itemData.itemId, out var config) && config.itemType == itemType)
                {
                    yield return itemData;
                }
            }
        }
    }
}
