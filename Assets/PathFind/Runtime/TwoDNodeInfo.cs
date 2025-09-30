/****************************************************************************//*
 *   寻路系统                                                                  *
 *    - 查找起点到终点可行走路径                                               *
 *    - 读取并使用阻碍点坐标                                                   *
 *    - 地图点状态判断                                                         *
 *                                                                             *
 *******************************************************************************/

using UnityEngine;

namespace UFrame.PathFind
{
    public class TwoDNodeInfo
    {
        public TwoDNodeInfo(Vector2Int _pos, float _cost, float _etCost, TwoDNodeInfo _parent)
        {
            pos = _pos;
            cost = _cost;
            etCost = _etCost;
            parent = _parent;
        }

        public float cost;
        public float etCost;
        public Vector2Int pos;
        public TwoDNodeInfo parent;
    }
}