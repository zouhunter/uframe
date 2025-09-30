/****************************************************************************//*
 *   寻路系统                                                                  *
 *    - 查找起点到终点可行走路径                                               *
 *    - 读取并使用阻碍点坐标                                                   *
 *    - 地图点状态判断                                                         *
 *                                                                             *
 *******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.PathFind
{
    public class TwoDPathFindCtrl : ITwoDPathFindCtrl
    {
        protected MapData m_collisionMap;
        protected float m_realMapWidth = 0f;
        protected float m_realMapHeight = 0f;
        protected float m_gridWidth = 0f;
        protected bool m_isInited = false;

        public virtual void Initialize(MapData collisionMap, float gridWidth)
        {
            m_collisionMap = collisionMap;
            this.m_gridWidth = gridWidth;
            m_realMapWidth = m_collisionMap.width * gridWidth;
            m_realMapHeight = m_collisionMap.height * gridWidth;
            m_isInited = true;
        }

        public virtual void Initialize(int width, int height,byte[] datas, float gridWidth)
        {
            m_collisionMap = new MapData();
            m_collisionMap.Initialize(width,height,datas);
            this.m_gridWidth = gridWidth;
            m_realMapWidth = m_collisionMap.width * gridWidth;
            m_realMapHeight = m_collisionMap.height * gridWidth;
            m_isInited = true;
        }

        public virtual void WalkableRequset(ref Vector3 from, ref Vector3 to, byte navMask)
        {
            Vector2 fromVec2 = new Vector2(from.x, from.z);
            Vector2 toVec2 = new Vector2(to.x, to.z);

            WalkableRequset(ref fromVec2, ref toVec2, navMask);
            from = new Vector3(fromVec2.x, from.y, fromVec2.y);
            to = new Vector3(toVec2.x, to.y, toVec2.y);
        }

        public virtual void WalkableRequset(ref Vector2 from, ref Vector2 to, byte navMask)
        {
            Vector2 targetVec = (to - from).normalized;
            while (!IsGridValide(PosToGrid(from), navMask) && Vector3.Dot(targetVec, (to - from).normalized) > 0)
                from += targetVec * m_gridWidth;

            while (!IsGridValide(PosToGrid(to), navMask) && Vector3.Dot(targetVec, (to - from).normalized) > 0)
                to -= targetVec * m_gridWidth;
        }

        public virtual Vector2[] FindPath(Vector2 from, Vector2 tar, byte navigateMask)
        {
            if (!m_isInited)
            {
                Debug.LogError("Cannot FindPath() befor init.");
            }

            // Find a nearest position for orig and target.
            WalkableRequset(ref from, ref tar, navigateMask);

            Vector2Int fromGrid = PosToGrid(from);
            Vector2Int tarGrid = PosToGrid(tar);

            // Search path
            HashSet<Vector2Int> usedPosSet = new HashSet<Vector2Int>() { fromGrid };
            List<TwoDNodeInfo> valideNodes = new List<TwoDNodeInfo>() { new TwoDNodeInfo(fromGrid, 0, GridDistance(fromGrid, tarGrid), null) };
            while (valideNodes.Count > 0)
            {
                var node = valideNodes[0];
                if (node.pos == tarGrid)
                {
                    // Oh my god you found it
                    List<Vector2> path = GetPathByFinalNode(node, navigateMask);
                    path.Add(tar);
                    return path.ToArray();
                }

                foreach (Vector2Int nearPos in GetNearGrids(node.pos))
                {
                    if (usedPosSet.Contains(nearPos))
                        continue;
                    else
                        usedPosSet.Add(nearPos);

                    if (!IsGridValide(nearPos, navigateMask))
                        continue;

                    TwoDNodeInfo childNode = new TwoDNodeInfo(nearPos, node.cost + GridDistance(node.pos, nearPos), GridDistance(nearPos, tarGrid), node);
                    float nodeValue = childNode.cost + childNode.etCost;
                    int nodeInsetIndex = valideNodes.FindIndex(x => x.cost + x.etCost > nodeValue);
                    if (nodeInsetIndex != -1)
                        valideNodes.Insert(nodeInsetIndex, childNode);
                    else
                        valideNodes.Add(childNode);
                }

                valideNodes.Remove(node);
            }

            return null;
        }

        public virtual bool IsPositionInMap(Vector2 pos)
        {
            Vector2Int grid = PosToGrid(pos);
            return m_collisionMap.IsGridValid(grid.x, grid.y);
        }

        public virtual bool IsPositionValide(Vector2 pos, byte navMask)
        {
            if (!m_isInited)
            {
                Debug.LogError("Cannot use IsPositionValide() before init.");
                return false;
            }

            return IsGridValide(PosToGrid(pos), navMask);
        }

        protected bool IsGridValide(Vector2Int grid, byte navMask)
        {
            if (!m_collisionMap.IsGridValid(grid.x, grid.y))
            {
                return false;
            }

            return (m_collisionMap[grid.x, grid.y] & navMask) != 0;
        }

        protected float GridDistance(Vector2Int g1, Vector2Int g2)
        {
            return Vector2.Distance(GridToPos(g1) * m_gridWidth, GridToPos(g2) * m_gridWidth);
        }

        protected List<Vector2> GetPathByFinalNode(TwoDNodeInfo node, byte navMask)
        {
            List<Vector2Int> pathNodes = new List<Vector2Int>();
            while (node.parent != null)
            {
                pathNodes.Add(node.pos);
                node = node.parent;
            }

            pathNodes = SmoothPath(pathNodes, navMask);

            List<Vector2> path = new List<Vector2>();
            foreach (Vector2Int curNode in pathNodes)
                path.Add(GridToPos(curNode));

            if (path.Count > 1)
                path.RemoveAt(path.Count - 1);

            path.Reverse();

            return path;
        }

        protected List<Vector2Int> SmoothPath(List<Vector2Int> path, byte navMask)
        {
            if (path.Count <= 1)
            {
                return path;
            }
            else if (path.Count <= 2 || !IsPathBlocked(path[0], path[path.Count - 1], navMask))
            {
                return new List<Vector2Int>() { path[0], path[path.Count - 1] };
            }
            else
            {
                List<Vector2Int> subList = new List<Vector2Int>();
                subList.AddRange(SmoothPath(path.GetRange(0, path.Count / 2), navMask));
                subList.AddRange(SmoothPath(path.GetRange(path.Count / 2, path.Count / 2), navMask));

                return subList;
            }
        }

        protected Vector2Int[] GetNearGrids(Vector2Int center)
        {
            return new Vector2Int[]{
            center + new Vector2Int(1, 1),
            center + new Vector2Int(0, 1),
            center + new Vector2Int(1, 0),
            center + new Vector2Int(-1, -1),
            center + new Vector2Int(0, -1),
            center + new Vector2Int(-1, 0),
            center + new Vector2Int(1, -1),
            center + new Vector2Int(-1, 1),
        };
        }

        protected Vector2Int PosToGrid(Vector2 pos)
        {
            return new Vector2Int(Mathf.FloorToInt((pos.x + 0.5f * m_realMapWidth) / m_gridWidth), Mathf.FloorToInt((pos.y + 0.5f * m_realMapHeight) / m_gridWidth));
        }

        protected Vector2 GridToPos(Vector2Int grid)
        {
            return new Vector2((grid.x + 0.5f) * m_gridWidth - 0.5f * m_realMapWidth, (grid.y + 0.5f) * m_gridWidth - 0.5f * m_realMapHeight);
        }

        protected bool IsPathBlocked(Vector2Int startPoint, Vector2Int endPoint, byte navMask)
        {
            if (startPoint.x > endPoint.x)
            {
                var tempPoint = endPoint;
                endPoint = startPoint;
                startPoint = tempPoint;
            }

            if (startPoint.x == endPoint.x)
            {
                // Two point is on a vertical line
                // Direct add the points on line
                int minY = Mathf.Min(startPoint.y, endPoint.y);
                int maxY = Mathf.Max(startPoint.y, endPoint.y);
                for (int i = minY; i <= maxY; ++i)
                {
                    if (!IsGridValide(new Vector2Int(startPoint.x, i), navMask))
                        return true;
                }

                return false;
            }

            Vector2 startPos = GridToPos(startPoint);
            Vector2 endPos = GridToPos(endPoint);

            Vector2 lineVec = (endPos - startPos).normalized;
            float slop = lineVec.y / lineVec.x;

            while (Vector2.Dot((endPos - startPos).normalized, lineVec) > 0.0001)
            {
                float curGridEdgX = (startPoint.x + 1) * m_gridWidth - 0.5f * m_realMapWidth;
                float xDelta = curGridEdgX - startPos.x;
                float yDelta = xDelta * slop;

                int endYIndex = (int)((startPos.y + yDelta + 0.5f * m_realMapHeight) / m_gridWidth);

                int minY = Mathf.Min(startPoint.y, endYIndex);
                int maxY = Mathf.Max(startPoint.y, endYIndex);
                for (int i = minY; i <= maxY; ++i)
                    if (!IsGridValide(new Vector2Int(startPoint.x, i), navMask))
                        return true;


                startPos.x += xDelta;
                startPos.y += yDelta;

                startPoint.x++;
                startPoint.y = endYIndex;
            }

            return false;
        }

    }
}