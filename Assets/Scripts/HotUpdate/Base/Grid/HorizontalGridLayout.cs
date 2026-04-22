using System.Collections.Generic;
using Core.AssetBundles.Management;
using UnityEngine;

namespace HotUpdate.Base.Grid
{
    /// <summary>
    /// 水平格子布局
    /// </summary>
    internal class HorizontalGridLayout<T, K> : GridLayout where K : Object, IGridBase<T>
    {
        // ---------- 数据与显示缓存 ----------
        private readonly Dictionary<int, PoolObject> _nowShowGridDic = new();  // 当前显示的格子字典，Key：数据索引，Value：对象池包装对象
        private readonly List<T> _dataList = new();                            // 全部数据列表

        // ---------- 上一次可见索引范围（用于判断回收） ----------
        private int oldMinIndex = -1;
        private int oldMaxIndex = -1;
        
        // 每列最大行数
        public int maxRow;

        /// <summary>
        /// 更新
        /// </summary>
        public void UpdateGrid()
        {
            // count小于等于sv的宽
            if (_dataList.Count * (_gridXSpace + _gridWidth) <= ((RectTransform)_sv.transform).sizeDelta.x)
            {
                // 禁用滚动
                _sv.horizontal = false;
                // 居中显示
                // 获取中间x位置
                var middlePosX = _content.sizeDelta.x / 2;
                // 数量为奇数
                if (_dataList.Count % 2 != 0)
                {
                    // 获取放置在中间位置的格子索引
                    var middleIndex = _dataList.Count / 2;
                    for (var i = 0; i < _dataList.Count; i++)
                    {
                        var poolObject = objectSpawner.Spawn<K>(AssetKeys.Itemcell, _content);
                        // 实例化格子对象
                        var gridInstance = poolObject.Obj;
                        // 格子索引小于放置在中间格子的索引，放置在中间格子的左边
                        if (i < middleIndex)
                        {
                            //(middlePosX - (gridWidth + xSpace) / 2)：中间格子中部水平位置
                            //(middleIndex - i)：偏移单位，索引从0开始，偏移单位逐渐减少
                            //(gridWidth + xSpace)：偏移单位为（格子宽度+水平间隙）
                            float pos = (middlePosX - (gridWidth + xSpace) / 2) - (middleIndex - i) * (gridWidth + xSpace);
                            //设置中间格子左边的位置
                            (gridInstance.transform as RectTransform).anchoredPosition = new Vector3(pos, 0);
                        }
                        //格子索引大于放置在中间格子的索引，放置在中间格子的右边
                        else if(i > middleIndex)
                        {
                            //(middlePosX - (gridWidth + xSpace) / 2)：中间格子中部水平位置
                            //(i - middleIndex)：偏移单位，索引从大于中间格子索引开始，偏移单位逐渐增减
                            //(gridWidth + xSpace)：偏移单位为（格子宽度+水平间隙）
                            float pos = (middlePosX - (gridWidth + xSpace) / 2) + (i - middleIndex) * (gridWidth + xSpace);
                            //设置中间格子右边的位置
                            (gridInstance.transform as RectTransform).anchoredPosition = new Vector3(pos, 0);
                        }
                        //格子索引等于放置在中间格子的索引，该索引格子就是中间格子
                        else
                        {
                            //设置中间格子的位置
                            (gridInstance.transform as RectTransform).anchoredPosition = new Vector3(middlePosX - (gridWidth + xSpace) / 2, 0);
                        }

                        //获取格子脚本
                        K grid = gridInstance.GetComponent<K>();
                        //初始化格子
                        grid.InitGrid(dataList[i]);
                        //存储格子
                        _gridList.Add(gridInstance);
                    }
                }
                //数量为偶数
                else
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        //实例化格子
                        GameObject gridInstance = GameObject.Instantiate(gridObj, _content, false);
                        //设置位置
                        //dataList.Count / 2获取左边放置的格子数量
                        if (i < dataList.Count / 2)
                        {
                            //(gridWidth + xSpace)：偏移单位为（格子宽度+水平间隙）
                            //((dataList.Count / 2 - i)：偏移单位，索引从0开始，偏移单位逐渐减少
                            //middlePosX - ...：从中部位置向左偏移多少个格子
                            (gridInstance.transform as RectTransform).anchoredPosition = new Vector3(middlePosX - ((dataList.Count / 2 - i) * (gridWidth + xSpace)), 0);
                        }
                        //右边放置的格子数量等于左边放置格子的数量
                        else
                        {
                            //(gridWidth + xSpace)：偏移单位为（格子宽度+水平间隙）
                            //((i + dataList.Count / 2)：偏移单位，放置在右边格子的索引偏移单位逐渐增加
                            //middlePosX + ...：从中部位置向右偏移多少个格子
                            (gridInstance.transform as RectTransform).anchoredPosition = new Vector3(middlePosX + ((i - dataList.Count / 2) * (gridWidth + xSpace)), 0);
                        }

                        //获取格子脚本
                        K grid = gridInstance.GetComponent<K>();
                        //初始化和格子
                        grid.InitGrid(dataList[i]);
                        //存储格子预设体
                        _gridList.Add(gridInstance);
                    }
                }
            }
            //count大于sv的宽
            else
            {
                //启用滚动
                _horizontalScrollRect.horizontal = true;
                //拓展conten的宽
                _content.sizeDelta = new Vector2(dataList.Count * (xSpace + gridWidth), _content.sizeDelta.y);
                //从左开始显示即可
                for (int i = 0; i < dataList.Count; i++)
                {
                    //实例化格子对象
                    GameObject gridInstance = GameObject.Instantiate(gridObj, _content, false);
                    //设置格子位置
                    (gridInstance.transform as RectTransform).anchoredPosition = new Vector2(i * (gridWidth + xSpace), 0);
                    //获取格子脚本
                    K grid = gridInstance.GetComponent<K>();
                    //初始化格子
                    grid.InitGrid(dataList[i]);
                    //存储格子
                    _gridList.Add(gridInstance);
                }
            }
        }
        
        public override (int minIndex, int maxIndex) CalcIndex()
        {
            return default;
        }

        public override Vector3 CalcPosition(int index)
        {
            return default;
        }

        public override void CalcContentSize(int dataCount)
        {

        }
    }
}
