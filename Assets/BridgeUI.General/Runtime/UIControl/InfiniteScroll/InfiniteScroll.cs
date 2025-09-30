using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfiniteScroll : ScrollRect
{
    #region 公共字段
    #endregion

    #region 私有字段
    /// <summary>
    /// 显示项处理方法
    /// </summary>
    Action<Transform, int> _displayCellHandler;

    /// <summary>
    /// 销毁项回调方法
    /// </summary>
    Action<Transform, int> _destroyCellCallback;

    /// <summary>
    /// 模板
    /// </summary>
    RectTransform _template;

    /// <summary>
    /// 视图节点
    /// </summary>
    RectTransform _rectViewport;

    /// <summary>
    /// 内容节点
    /// </summary>
    RectTransform _rectContent;

    /// <summary>
    /// 网格布局
    /// </summary>
    GridLayoutGroup _gridLayout;

    /// <summary>
    /// 总数
    /// </summary>
    int _totalNum;

    /// <summary>
    /// 最大显示项数
    /// </summary>
    Vector2Int _maxDisplayCellNum2;

    /// <summary>
    /// 最大完整显示项数
    /// </summary>
    Vector2Int _maxFullDisplayCellNum2;

    /// <summary>
    /// 目标数量
    /// </summary>
    int _targetNum;

    /// <summary>
    /// 映射每一个有效节点对应的下标
    /// </summary>
    List<int> _listMappingIdx = new List<int>();

    /// <summary>
    /// 是否垂直
    /// </summary>
    bool _isVerti;

    /// <summary>
    /// 列行数 
    /// </summary>
    Vector2Int _boundsNum;

    /// <summary>
    /// 是否初始化
    /// </summary>
    bool _isInit;

    /// <summary>
    /// 模块名
    /// </summary>
    string _module;

    /// <summary>
    /// 资源名
    /// </summary>
    string _assetName;
    #endregion

    #region 默认回调
    #endregion

    #region 继承方法
    /// <summary>
    /// 设置内容位置
    /// </summary>
    /// <param name="position"></param>
    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        base.SetContentAnchoredPosition(position);

        // 是否没初始化
        if (!_isInit)
        {
            return;
        }

        // 刷新位置
        RefreshPos();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 显示列表
    /// </summary>
    /// <param name="num">总项数</param>
    /// <param name="displayCellHandler">显示处理方法</param>
    /// <param name="defaultFocusIdx">默认焦点下标</param>
    /// <param name="spacing">项之间间隙</param>
    public void ShowScroll(int num, Action<Transform, int> displayCellHandler, int defaultFocusIdx = 0, Vector2 spacing = default)
    {
        if (num < 0)
        {
            Debug.Log("[InfiniteScroll][num必须大于等于0]");
            return;
        }

        // 停止滚动
        velocity = Vector2.zero;

        // 记录
        _isInit = true;
        _totalNum = num;
        _displayCellHandler = displayCellHandler;

        // 重置
        _listMappingIdx.Clear();

        // 设置模板
        SetTemplete();

        // 获得组件
        _rectViewport = viewport.GetComponent<RectTransform>();
        _rectContent = content.GetComponent<RectTransform>();
        _gridLayout = content.GetComponent<GridLayoutGroup>();

        // 是否没有网格组件
        if (_gridLayout == null)
        {
            // 添加组件
            _gridLayout = content.gameObject.AddComponent<GridLayoutGroup>();
        }

        // 是否垂直
        _isVerti = !horizontal || vertical;
        if (_isVerti)
        {
            _gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
        else
        {
            _gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
        }

        // 设置布局的间隙和项宽高
        if (spacing != default)
        {
            _gridLayout.spacing = spacing;
        }
        _gridLayout.cellSize = _template.sizeDelta;
        _gridLayout.padding = new RectOffset();

        // 计算最大显示项数
        float widthC = _rectViewport.rect.width / (_template.rect.width + _gridLayout.spacing.x);
        float heightC = _rectViewport.rect.height / (_template.rect.height + _gridLayout.spacing.y);
        _maxDisplayCellNum2.x = Mathf.CeilToInt(widthC);
        _maxDisplayCellNum2.y = Mathf.CeilToInt(heightC);
        _maxFullDisplayCellNum2.x = Mathf.Max(1, Mathf.FloorToInt(widthC));
        _maxFullDisplayCellNum2.y = Mathf.Max(1, Mathf.FloorToInt(heightC));
        if (_isVerti)
        {
            if (_rectViewport.rect.width - _maxFullDisplayCellNum2.x * (_template.rect.width + _gridLayout.spacing.x) >= _template.rect.width)
            {
                _maxFullDisplayCellNum2.x++;
            }
        }
        else
        {
            if (_rectViewport.rect.height - _maxFullDisplayCellNum2.y * (_template.rect.height + _gridLayout.spacing.y) >= _template.rect.height)
            {
                _maxFullDisplayCellNum2.y++;
            }
        }

        // 计算总共的行列
        if (_isVerti)
        {
            _boundsNum.x = _maxFullDisplayCellNum2.x;
            _boundsNum.y = Mathf.CeilToInt((float)_totalNum / _maxFullDisplayCellNum2.x);
        }
        else
        {
            _boundsNum.y = _maxFullDisplayCellNum2.y;
            _boundsNum.x = Mathf.CeilToInt((float)_totalNum / _maxFullDisplayCellNum2.y);
        }
        _boundsNum.x = Mathf.Max(_boundsNum.x, 0);
        _boundsNum.y = Mathf.Max(_boundsNum.y, 0);

        // 目标数量
        if (_isVerti)
        {
            _targetNum = Mathf.Min(_totalNum, Mathf.RoundToInt(_maxFullDisplayCellNum2.x * (_maxDisplayCellNum2.y + 1)));
        }
        else
        {
            _targetNum = Mathf.Min(_totalNum, Mathf.RoundToInt((_maxDisplayCellNum2.x + 1) * _maxFullDisplayCellNum2.y));
        }
        
        // 如果不足则拷贝
        int need = Mathf.Max(0, _targetNum - content.childCount);
        Transform cell;
        for (int i = 0; i < need; i++)
        {
            cell = GetCell();
        }

        // 遍历所有子节点
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            // 获得节点
            cell = content.GetChild(i);

            // 是否有效
            if (i < _targetNum)
            {
                // 设置显隐
                cell.gameObject.SetActive(true);

                // 设置名字
                cell.name = "" + i;
            }
            else
            {
                // 回调
                int t = i;
                _destroyCellCallback?.Invoke(cell, t);

                // 删除
                if (cell != _template.transform)
                {
                    // 回收或销毁
                    Destroy(cell.gameObject);
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
            }
        }

        // 设置内容节点锚点
        Vector2 anchor = new Vector2(0, 1);
        _rectContent.anchorMin = anchor;
        _rectContent.anchorMax = anchor;
        _rectContent.pivot = anchor;
        _rectContent.anchoredPosition = Vector2.zero;

        int row = _boundsNum.y;
        int col = _boundsNum.x;
        if (num == 0)
        {
            // 设置内容节点宽高
            _rectContent.sizeDelta = Vector2.zero;
        }
        else
        {
            // 设置内容节点宽高
            if (_isVerti)
            {
                _rectContent.sizeDelta = new Vector2(_rectViewport.rect.width, row * _template.rect.height + (row - 1) * _gridLayout.spacing.y);
            }
            else
            {
                _rectContent.sizeDelta = new Vector2(col * _template.rect.width + (col - 1) * _gridLayout.spacing.x, _rectViewport.rect.height);
            }
        }

        // 是否有焦点
        if (defaultFocusIdx > 0)
        {
            // 看到对应下标
            FocusAt(defaultFocusIdx, true);
        }
        else
        {
            // 刷新位置
            RefreshPos();
        }
    }

    /// <summary>
    /// 看到对应下标
    /// </summary>
    /// <param name="idx">数据下标</param>
    /// <param name="isAsFirst">是否必须作为第一个,否则完整可见就不操作</param>
    public void FocusAt(int idx, bool isAsFirst)
    {
        // 是否没初始化
        if (!_isInit)
        {
            return;
        }

        idx = Mathf.Clamp(idx, 0, _totalNum - 1);

        // 停止滚动
        velocity = Vector2.zero;

        // 是否该节点完整可见
        if (CheckIdxIsFullVisible(idx, out int outDir))
        {
            // 是否必须第一个
            if (isAsFirst)
            {
                // 遍历第一行或第一列
                int num = _isVerti ? _maxFullDisplayCellNum2.x : _maxFullDisplayCellNum2.y;
                for (int i = 0; i < num; i++)
                {
                    // 是否已经第一个
                    if (_listMappingIdx[i] == idx)
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        // 是否必须第一个 或 从上面超出
        if (isAsFirst || outDir == 1)
        {
            // 设置显示到第一个
            if (_isVerti)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, 
                    (_template.rect.height + _gridLayout.spacing.y) * (idx / _maxFullDisplayCellNum2.x));
            }
            else
            {
                content.anchoredPosition = new Vector2(-(_template.rect.width + _gridLayout.spacing.x) * (idx / _maxFullDisplayCellNum2.y), 
                    content.anchoredPosition.y);
            }
        }
        else
        {
            // 设置到底部
            if (_isVerti)
            {
                // 计算底部的行数
                int row = Mathf.Max(0, Mathf.Min(_boundsNum.y - (idx / _maxFullDisplayCellNum2.x)) - 1);

                content.anchoredPosition = new Vector2(content.anchoredPosition.x, 
                    _rectContent.rect.height - _rectViewport.rect.height - (_template.rect.height + _gridLayout.spacing.y) * row);
            }
            else
            {
                // 计算右侧的列数
                int col = Mathf.Max(0, Mathf.Min(_boundsNum.x - (idx / _maxFullDisplayCellNum2.y)) - 1);

                content.anchoredPosition = new Vector2(-_rectContent.rect.width + _rectViewport.rect.width + (_template.rect.width + _gridLayout.spacing.x) * col,
                    content.anchoredPosition.y);
            }
        }

        // 刷新位置
        RefreshPos();
    }

    /// <summary>
    /// 检测一个节点是否完整可见
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="outDir">超出方向 -1下或右 1上或左</param>
    /// <returns></returns>
    public bool CheckIdxIsFullVisible(int idx, out int outDir)
    {
        // 是否没初始化
        if (!_isInit)
        {
            outDir = 1;
            return false;
        }

        // 没有数据
        if (_listMappingIdx.Count == 0)
        {
            outDir = 1;
            return false;
        }

        // 获得最大一个的下标
        int max = _listMappingIdx[_listMappingIdx.Count - 1];

        // 是否下标小于最小的
        if (idx < _listMappingIdx[0])
        {
            // 从上左超出
            outDir = 1;
            return false;
        }   
        // 是否大于最大的
        else if (idx > max)
        {
            // 从下右超出
            outDir = -1;
            return false;
        }

        // 获得显示的下标
        int cellIdx = _listMappingIdx.Count - (max - idx) - 1;

        // 获得对应节点
        RectTransform rectCell = content.GetChild(cellIdx).GetComponent<RectTransform>();

        if (_isVerti)
        {
            // 是否该节点顶部超出顶
            if ((rectCell.anchoredPosition.y + rectCell.rect.height * 0.5f + _rectContent.anchoredPosition.y) > float.Epsilon)
            {
                outDir = 1;
                return false;
            }
            // 是否该节点底部超出底
            else if (-_rectViewport.rect.height - (rectCell.anchoredPosition.y - rectCell.rect.height * 0.5f + _rectContent.anchoredPosition.y) > float.Epsilon)
            {
                outDir = -1;
                return false;
            }
        }
        else
        {
            // 是否该节点左边超出
            if ((rectCell.anchoredPosition.x - rectCell.rect.width * 0.5f + _rectContent.anchoredPosition.x) < float.Epsilon)
            {
                outDir = 1;
                return false;
            }
            // 是否该节点右边超出
            else if ((rectCell.anchoredPosition.x + rectCell.rect.width * 0.5f + _rectContent.anchoredPosition.x) - _rectViewport.rect.width > float.Epsilon)
            {
                outDir = -1;
                return false;
            }
        }

        outDir = 0;
        return true;
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="isStopScroll">是否停止滚动</param>
    public void Refresh(bool isStopScroll = true)
    {
        // 是否没初始化
        if (!_isInit)
        {
            return;
        }

        // 停止滚动
        if (isStopScroll)
        {
            velocity = Vector2.zero;
        }

        // 遍历所有有效节点
        int idx;
        Transform cell;
        for (int i = 0; i < _targetNum; i++)
        {
            // 获得项
            cell = content.GetChild(i);

            // 是否显示
            if (cell.gameObject.activeSelf)
            {
                // 获得数据
                idx = cell.GetComponent<InfiniteScrollCell>().dataIdx;

                // 回调显示
                _displayCellHandler?.Invoke(cell, idx);
            }
        }
    }
    
    /// <summary>
    /// 设置加载路径
    /// </summary>
    /// <param name="module"></param>
    /// <param name="assetName"></param>
    public void SetLoadPath(string module, string assetName)
    {
        _module = module;
        _assetName = assetName;
    }

    /// <summary>
    /// 设置销毁项回调
    /// </summary>
    /// <param name="destroyCellCallback"></param>
    public void SetDestroyuCallback(Action<Transform, int> destroyCellCallback)
    {
        _destroyCellCallback = destroyCellCallback;
    }
    #endregion

    #region 其他方法
    /// <summary>
    /// 刷新位置
    /// </summary>
    public void RefreshPos()
    {
        List<Transform> listCell = new List<Transform>();
        int refreshStartIdx = 0;
        int refreshEndIdx = _targetNum - 1;

        // 前方的行数 及 视图内可显示的第一个行下标
        int firstRowIdx = Mathf.FloorToInt(Mathf.Max(0, _rectContent.anchoredPosition.y) / (_template.rect.height + _gridLayout.spacing.y));
        firstRowIdx = Mathf.Min(firstRowIdx, Mathf.Max(0, _boundsNum.y - _maxDisplayCellNum2.y - 1));

        // 前方的列数 及 视图内可显示的第一个列下标
        int firstColIdx = Mathf.FloorToInt(Mathf.Max(0, -_rectContent.anchoredPosition.x) / (_template.rect.width + _gridLayout.spacing.x));
        firstColIdx = Mathf.Min(firstColIdx, Mathf.Max(0, _boundsNum.x - _maxDisplayCellNum2.x - 1));

        int firstIdx = 0;
        if (_isVerti)
        {
            firstIdx = firstRowIdx * _boundsNum.x + firstColIdx;
        }
        else
        {
            firstIdx = firstColIdx * _boundsNum.y + firstRowIdx;
        }

        // 是否没有映射数据
        if (_listMappingIdx.Count == 0)
        {
            for (int i = 0; i < _targetNum; i++)
            {
                // 添加映射下标
                _listMappingIdx.Add(firstIdx + i);
            }
        }
        else
        {
            for (int i = 0; i < _targetNum; i++)
            {
                // 寻找之前的映射中是否有当前的第一个下标
                if (_listMappingIdx[i] == firstIdx)
                {
                    // 是否处于第一位
                    if (i == 0)
                    {
                        // 完全相同 不处理
                        return;
                    }

                    // 是否位置超过或等于一半
                    if (i >= _targetNum / 2)
                    {
                        // 反向置顶该位置及之后的
                        for (int t = _targetNum - 1; t >= i; t--)
                        {
                            listCell.Add(content.GetChild(t));
                        }
                        foreach (var item in listCell)
                        {
                            item.SetAsFirstSibling();
                        }
                    }
                    else
                    {
                        // 置底该位置之前的
                        for (int t = 0; t < i; t++)
                        {
                            listCell.Add(content.GetChild(t));
                        }
                        foreach (var item in listCell)
                        {
                            item.SetAsLastSibling();
                        }
                    }

                    // 设置刷新下标范围
                    refreshStartIdx = _targetNum - i;
                    refreshEndIdx = _targetNum - 1;

                    break;
                }
                // 寻找之前的映射中是否有当前的最后一个下标
                else if (_listMappingIdx[i] == firstIdx + _targetNum - 1)
                {
                    // 是否位置超过或等于一半
                    if (i >= _targetNum / 2)
                    {
                        // 该位置之后的置顶
                        for (int t = i + 1; t < _targetNum; t++)
                        {
                            listCell.Add(content.GetChild(t));
                        }
                        foreach (var item in listCell)
                        {
                            item.SetAsFirstSibling();
                        }
                    }
                    else
                    {
                        // 正向置底该位置及之前的
                        for (int t = 0; t <= i; t++)
                        {
                            listCell.Add(content.GetChild(t));
                        }
                        foreach (var item in listCell)
                        {
                            item.SetAsLastSibling();
                        }
                    }

                    // 设置刷新下标范围
                    refreshStartIdx = 0;
                    refreshEndIdx = _targetNum - i - 2;

                    break;
                }
            }
        }

        // 设置前面位置
        if (_isVerti)
        {
            _gridLayout.padding.top = Mathf.CeilToInt((_template.rect.height + _gridLayout.spacing.y) * firstRowIdx);
        }
        else
        {
            _gridLayout.padding.left = Mathf.CeilToInt((_template.rect.width + _gridLayout.spacing.x) * firstColIdx);
        }

        // 强制刷新
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectContent);

        // 遍历所有有效节点
        int idx;
        Transform cell;
        for (int i = 0; i < _targetNum; i++)
        {
            // 获得项
            cell = content.GetChild(i);

            // 计算下标
            idx = firstIdx + i;

            // 设置映射
            _listMappingIdx[i] = idx;

            if (i >= refreshStartIdx && i <= refreshEndIdx)
            {
                // 是否数据合法
                if (idx >= 0 && idx < _totalNum)
                {
                    // 设置数据
                    cell.GetComponent<InfiniteScrollCell>().dataIdx = idx;

                    // 回调显示
                    _displayCellHandler?.Invoke(cell, idx);

                    // 显示
                    cell.gameObject.SetActive(true);
                }
                else
                {
                    // 隐藏
                    cell.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 设置模板
    /// </summary>
    void SetTemplete()
    {
        if (_template != null)
        {
            // 提到第一个
            _template.SetAsFirstSibling();

            return;
        }

        // 获取第一个
        _template = content.GetChild(0).GetComponent<RectTransform>();

        // 添加组件
        AddComp(_template);

        // 显示
        _template.gameObject.SetActive(_totalNum > 0);
    }

    /// <summary>
    /// 获取项
    /// </summary>
    /// <returns></returns>
    Transform GetCell()
    {
        // 拷贝
        Transform cell = Instantiate(_template);

        // 设置节点
        cell.SetParent(content, false);

        return cell;
    }

    /// <summary>
    /// 添加组件
    /// </summary>
    /// <param name="cell"></param>
    void AddComp(Transform cell)
    {
        var cellComp = cell.GetComponent<InfiniteScrollCell>();
        if (cellComp == null)
        {
            cell.gameObject.AddComponent<InfiniteScrollCell>();
        }
    }
    #endregion
}
