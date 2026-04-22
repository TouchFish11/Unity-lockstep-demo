using System;
using System.Collections.Generic;
using Core.DI;
using Core.Log;
using Core.Pool;
using HotUpdate.Base.Grid;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HotUpdate.Base.Factory
{
    /// <summary>
    /// 格子生成器构建器
    /// </summary>
    /// <typeparam name="T">格子展示的数据类型</typeparam>
    /// <typeparam name="K">格子组件类型，必须继承自 Object 并实现 IGridBase&lt;T&gt; 接口</typeparam>
    public class GridGeneratorBuilder<T, K> where K : Object, IGridBase<T>
    {
        [Inject] private IPoolManager _poolManager;
        
        private static readonly Dictionary<EGridLayout, GridLayout> _gridLayouts = new();
        // 构建的对象
        private GridGenerator<T, K> _gridGenerator;

        static GridGeneratorBuilder()
        {
            _gridLayouts.Add(EGridLayout.Horizontal, new HorizontalGridLayout());
            _gridLayouts.Add(EGridLayout.Vertical, new VerticalGridLayout());
        }
        
        public GridGeneratorBuilder<T, K> CreateGenerator(EGridLayout gridLayout)
        {
            _gridGenerator = _poolManager.GetData<GridGenerator<T, K>>();
            _gridGenerator.gridLayout = _gridLayouts.GetValueOrDefault(gridLayout);
            return this;
        }
        
        public GridGeneratorBuilder<T, K> SetParent(ScrollRect scrollRect)
        {
            _gridGenerator.gridLayout._sv = scrollRect;
            _gridGenerator.gridLayout._content = scrollRect.content;
            return this;
        }

        public GridGeneratorBuilder<T, K> SetGridSize(float gridWidth, float gridHeight)
        {
            _gridGenerator.gridLayout._gridWidth = gridWidth;
            _gridGenerator.gridLayout._gridHeight = gridHeight;
            return this;
        }
        
        public GridGeneratorBuilder<T, K> SetGridSpace(float gridXSpace, float gridYSpace)
        {
            _gridGenerator.gridLayout._gridXSpace = gridXSpace;
            _gridGenerator.gridLayout._gridYSpace = gridYSpace;
            return this;
        }
        
        public GridGeneratorBuilder<T, K> SetColumn(int maxCol)
        {
            if (_gridGenerator.gridLayout is VerticalGridLayout verticalGridLayout)
                verticalGridLayout.maxCol = maxCol;
            else
                Logger.LogError($"{nameof(GridGeneratorBuilder<T, K>)} can only be used with {nameof(VerticalGridLayout)}");
            return this;
        }
        
        public GridGeneratorBuilder<T, K> SetRow(int maxRow)
        {
            if (_gridGenerator.gridLayout is HorizontalGridLayout horizontalGridLayout)
                horizontalGridLayout.maxRow = maxRow;
            else
                Logger.LogError($"{nameof(GridGeneratorBuilder<T, K>)} can only be used with {nameof(HorizontalGridLayout)}");
            return this;
        }
        
        /// <summary>
        /// 设置点击事件，让格子监听该事件
        /// </summary>
        /// <param name="callback"></param>
        public GridGeneratorBuilder<T, K> SetClick(Action<T> callback)
        {
            _gridGenerator.SetClick(callback);
            return this;
        }
        
        public GridGeneratorBuilder<T, K> SetDatas(List<T> dataList)
        {
            _gridGenerator.SetDatas(dataList);
            return this;
        }
        
        /// <summary>
        /// 设置选中的格子索引，当对应索引的格子创建完毕后会自动执行其点击事件
        /// 执行完后索引重置，需重新调用方法设置
        /// </summary>
        /// <param name="index"></param>
        public GridGeneratorBuilder<T, K> SetSelectIndex(int index)
        {
            _gridGenerator.SetSelectIndex(index);
            return this;
        }

        public GridGenerator<T, K> Build()
        {
            _gridGenerator.CalcContentSize();
            return _gridGenerator;
        }
        
        public static GridGeneratorBuilder<T, K> Create()
        {
            return DIContainer.Create<GridGeneratorBuilder<T, K>>();
        }
    }
}
