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
    public interface ITwoDPathFindCtrl
    {
        void Initialize(MapData collisionMap, float gridWidth);
        void Initialize(int width, int height,byte[] datas, float gridWidth);
        void WalkableRequset(ref Vector2 from, ref Vector2 to, byte navMask);
        void WalkableRequset(ref Vector3 from, ref Vector3 to, byte navMask);
        Vector2[] FindPath(Vector2 from, Vector2 tar, byte navMask);
        bool IsPositionInMap(Vector2 pos);
        bool IsPositionValide(Vector2 pos, byte navMask);
    }
}
