using System;
using System.Collections.Generic;
using Core.AssetBundles.Management;
using Core.DI;
using Core.Pool;
using UnityEngine;
using Logger = Core.Log.Logger;
using Object = UnityEngine.Object;

namespace HotUpdate.Base.Grid
{
    /// <summary>
    /// 可见格子生成器（虚拟列表核心实现）
    /// 仅创建和显示当前视口内的格子，超出视口的格子回收至对象池，支持海量数据的高性能滚动。
    /// </summary>
    /// <typeparam name="T">格子展示的数据类型</typeparam>
    /// <typeparam name="K">格子组件类型，必须继承自 Object 并实现 IGridBase&lt;T&gt; 接口</typeparam>
    public sealed class GridGenerator<T, K> : IPoolData where K : Object, IGridBase<T>
    {
        // ---------- 数据与显示缓存 ----------
        private readonly Dictionary<int, PoolObject> _nowShowGridDic = new();  // 当前显示的格子字典，Key：数据索引，Value：对象池包装对象
        private readonly List<T> _dataList = new();                            // 全部数据列表

        // ---------- 上一次可见索引范围（用于判断回收） ----------
        private int oldMinIndex = -1;
        private int oldMaxIndex = -1;
        
        // ---------- 依赖注入 ----------
        [Inject] private ObjectSpawner _objectSpawner;   // 对象生成器（支持异步实例化与对象池）

        // 事件注册
        private Action<T> _callback;
        // 当前选中的数据索引，-1 表示无选中
        private int _selectedIndex = -1;
        // 当前布局类型
        internal GridLayout gridLayout;
        
        /// <summary>
        /// 更新可见格子（通常在 ScrollRect.onValueChanged 事件或 Update 中调用）
        /// </summary>
        /// <param name="comparison">可选的数据排序委托，每次刷新前会调用</param>
        public void UpdateGrid(Comparison<T> comparison = null)
        {
            // 计算索引
            var (minIndex, maxIndex) = gridLayout.CalcIndex();
            
            // 边界保护：不能超出数据范围
            if (minIndex < 0)
                minIndex = 0;
            if (maxIndex >= _dataList.Count)
                maxIndex = _dataList.Count - 1;
            
            // 与上一次索引范围比较，回收移出视口的格子
            if (minIndex != oldMinIndex || maxIndex != oldMaxIndex)
            {
                // 向上滑动（内容向上，minIndex 变大）：回收顶部移出的格子
                // 范围：oldMinIndex 到 minIndex-1
                for (var i = oldMinIndex; i < minIndex; i++)
                {
                    if (_nowShowGridDic.TryGetValue(i, out var poolObj))
                    {
                        // 放回对象池
                        poolObj.Collect();
                        _nowShowGridDic.Remove(i);
                    }
                }

                // 向下滑动（内容向下，maxIndex 变小）：回收底部移出的格子
                // 范围：maxIndex+1 到 oldMaxIndex
                // 注意从 maxIndex+1 开始，因为 maxIndex 是当前可见的最后一个索引，必须保留
                for (var i = maxIndex + 1; i <= oldMaxIndex; i++)
                {
                    if (_nowShowGridDic.TryGetValue(i, out var poolObj))
                    {
                        poolObj.Collect();
                        _nowShowGridDic.Remove(i);
                    }
                }
            }

            // 记录当前索引范围为上一次范围，供下一帧使用
            oldMinIndex = minIndex;
            oldMaxIndex = maxIndex;

            // 排序数据（可选）
            if (comparison != null)
                _dataList.Sort(comparison);

            // 创建新进入视口的格子
            for (var i = minIndex; i <= maxIndex; ++i)
            {
                // 尝试向字典添加占位（若已存在则跳过，防止重复创建）
                if (!_nowShowGridDic.TryAdd(i, default))
                    continue;

                // 异步创建格子（字典中已占位 null，回调成功后会替换为实际对象）
                CreateGrid(i);
            }
        }

        public void SetDatas(List<T> datas)
        {
            _dataList.Clear();
            _dataList.AddRange(datas);
        }
        
        /// <summary>
        /// 设置点击事件，让格子监听该事件
        /// </summary>
        /// <param name="callback"></param>
        public void SetClick(Action<T> callback)
        {
            _callback = callback;
        }
        
        /// <summary>
        /// 设置选中的格子索引，当对应索引的格子创建完毕后会自动执行其点击事件
        /// 执行完后索引重置，需重新调用方法设置
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            _selectedIndex = index;
        }
        
        /// <summary>
        /// 清空所有格子
        /// </summary>
        public void ClearGrids()
        {
            foreach (var poolObject in _nowShowGridDic.Values)
            {
                poolObject.Collect();
            }
            _nowShowGridDic.Clear();
        }

        /// <summary>
        /// 计算内容总大小，要在最后调用计算
        /// </summary>
        internal void CalcContentSize()
        {
            gridLayout.CalcContentSize(_dataList.Count);
        }
        
        /// <summary>
        /// 异步创建指定索引的格子
        /// </summary>
        /// <param name="index">数据索引</param>
        private async void CreateGrid(int index)
        {
            try
            {
                // 异步从对象池获取格子实例（自动处理实例化、激活、父节点设置）
                var pos = gridLayout.CalcPosition(index);
                var poolObj = await _objectSpawner.SpawnAsync<K>(AssetKeys.Itemcell, gridLayout._content, pos, Quaternion.identity);
                // 初始化格子数据
                poolObj.Obj.InitGrid(_dataList[index]);
                // 二次确认：异步加载期间该索引是否仍有效（未被回收）
                if (_nowShowGridDic.ContainsKey(index))
                {
                    // 有效：将实际对象替换占位
                    _nowShowGridDic[index] = poolObj;
                    // 注册交互事件
                    poolObj.Obj.OnClick += _callback;
                    // 判断是否需要默认选中该索引的格子
                    if(_selectedIndex != -1 &&  _selectedIndex == index)
                    {
                        poolObj.Obj.TriggerClick();
                        _selectedIndex = -1;
                    }
                }
                else
                {
                    // 无效：说明在异步等待期间该索引已被回收，直接将对象放回池子
                    poolObj.Collect();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{nameof(GridGenerator<T, K>)}: {e.Message}");
            }
        }
        
        /// <summary>
        /// 重置数据（实现 IPoolData 接口，用于对象池回收生成器自身时清理引用）
        /// </summary>
        void IPoolData.ResetData()
        {
            ClearGrids();
        }
    }
}