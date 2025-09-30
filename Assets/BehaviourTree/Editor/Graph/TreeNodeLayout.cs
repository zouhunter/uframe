//*************************************************************************************
//* 作    者： 
//* 创建时间： 2025-04-13
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UFrame.BehaviourTree;

namespace UFrame.BehaviourTree
{
    public class TreeNode
    {
        public string Name { get; set; }
        public List<TreeNode> Children { get; set; }
    }

    public class TreeNodeLayout
    {
        public class DrawTreeNode
        {
            public TreeInfo Info { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public List<DrawTreeNode> Children { get; set; }
            public DrawTreeNode Parent { get; set; }
            public DrawTreeNode Thread { get; set; }
            public double Mod { get; set; }
            public DrawTreeNode Ancestor { get; set; }
            public double Change { get; set; }
            public double Shift { get; set; }
            private DrawTreeNode _lmostSibling;
            public int Number { get; set; }

            public DrawTreeNode(TreeInfo info, DrawTreeNode parent = null, int depth = 0, int number = 1)
            {
                Info = info;
                X = -1;
                Y = depth;
                Children = new List<DrawTreeNode>();

                // 处理子树
                if (info.subTrees != null)
                {
                    for (int i = 0; i < info.subTrees.Count; i++)
                    {
                        if (info.subTrees[i] == null)
                            continue;
                        Children.Add(new DrawTreeNode(info.subTrees[i], this, depth + 1, i + 1));
                    }
                }

                Parent = parent;
                Thread = null;
                Mod = 0;
                Ancestor = this;
                Change = 0;
                Shift = 0;
                _lmostSibling = null;
                Number = number;
            }

            public DrawTreeNode Right()
            {
                return Thread ?? (Children.Count > 0 ? Children[Children.Count - 1] : null);
            }

            public DrawTreeNode Left()
            {
                return Thread ?? (Children.Count > 0 ? Children[0] : null);
            }

            public DrawTreeNode LeftBrother()
            {
                DrawTreeNode n = null;
                if (Parent != null)
                {
                    foreach (var node in Parent.Children)
                    {
                        if (node == this)
                            return n;
                        n = node;
                    }
                }
                return n;
            }

            public DrawTreeNode GetLmostSibling()
            {
                if (_lmostSibling == null && Parent != null && this != Parent.Children[0])
                {
                    _lmostSibling = Parent.Children[0];
                }
                return _lmostSibling;
            }

            public DrawTreeNode LeftmostSibling => GetLmostSibling();
        }

        private static DrawTreeNode Firstwalk(DrawTreeNode v, float distance = 1)
        {
            if (v.Children.Count == 0)
            {
                if (v.LeftmostSibling != null)
                {
                    v.X = v.LeftBrother().X + distance;
                }
                else
                {
                    v.X = 0;
                }
            }
            else
            {
                var defaultAncestor = v.Children[0];
                foreach (var child in v.Children)
                {
                    Firstwalk(child);
                    defaultAncestor = Apportion(child, defaultAncestor, distance);
                }
                ExecuteShifts(v);

                double midpoint = (v.Children[0].X + v.Children[v.Children.Count - 1].X) / 2;
                var w = v.LeftBrother();
                if (w != null)
                {
                    v.X = w.X + distance;
                    v.Mod = v.X - midpoint;
                }
                else
                {
                    v.X = midpoint;
                }
            }
            return v;
        }

        private static DrawTreeNode Apportion(DrawTreeNode v, DrawTreeNode defaultAncestor, float distance)
        {
            var leftBrother = v.LeftBrother();
            if (leftBrother != null)
            {
                var vInnerRight = v;
                var vOuterRight = v;
                var vInnerLeft = leftBrother;
                var vOuterLeft = v.LeftmostSibling;

                var sInnerRight = v.Mod;
                var sOuterRight = v.Mod;
                var sInnerLeft = vInnerLeft.Mod;
                var sOuterLeft = vOuterLeft.Mod;

                while (vInnerLeft.Right() != null && vInnerRight.Left() != null)
                {
                    vInnerLeft = vInnerLeft.Right();
                    vInnerRight = vInnerRight.Left();
                    vOuterLeft = vOuterLeft.Left();
                    vOuterRight = vOuterRight.Right();

                    vOuterRight.Ancestor = v;

                    var shift = vInnerLeft.X + sInnerLeft - (vInnerRight.X + sInnerRight) + distance;
                    if (shift > 0)
                    {
                        var ancestor = Ancestor(vInnerLeft, v, defaultAncestor);
                        MoveSubtree(ancestor, v, shift);
                        sInnerRight += shift;
                        sOuterRight += shift;
                    }

                    sInnerLeft += vInnerLeft.Mod;
                    sInnerRight += vInnerRight.Mod;
                    sOuterLeft += vOuterLeft.Mod;
                    sOuterRight += vOuterRight.Mod;
                }

                if (vInnerLeft.Right() != null && vOuterRight.Right() == null)
                {
                    vOuterRight.Thread = vInnerLeft.Right();
                    vOuterRight.Mod += sInnerLeft - sOuterRight;
                }
                else if (vInnerRight.Left() != null && vOuterLeft.Left() == null)
                {
                    vOuterLeft.Thread = vInnerRight.Left();
                    vOuterLeft.Mod += sInnerRight - sOuterLeft;
                    defaultAncestor = v;
                }
            }
            return defaultAncestor;
        }

        private static void MoveSubtree(DrawTreeNode wl, DrawTreeNode wr, double shift)
        {
            var subtrees = wr.Number - wl.Number;
            wr.Change -= shift / subtrees;
            wr.Shift += shift;
            wl.Change += shift / subtrees;
            wr.X += shift;
            wr.Mod += shift;
        }

        private static void ExecuteShifts(DrawTreeNode v)
        {
            double shift = 0;
            double change = 0;
            for (int i = v.Children.Count - 1; i >= 0; i--)
            {
                var w = v.Children[i];
                w.X += shift;
                w.Mod += shift;
                change += w.Change;
                shift += w.Shift + change;
            }
        }

        private static DrawTreeNode Ancestor(DrawTreeNode vInnerLeft, DrawTreeNode v, DrawTreeNode defaultAncestor)
        {
            return v.Parent.Children.Contains(vInnerLeft.Ancestor) ? vInnerLeft.Ancestor : defaultAncestor;
        }

        private static float SecondWalk(DrawTreeNode v, double m = 0, int depth = 0, float? min = null)
        {
            v.X += m;
            v.Y = depth;

            if (!min.HasValue || v.X < min.Value)
            {
                min = (float)v.X;
            }

            foreach (var child in v.Children)
            {
                min = SecondWalk(child, m + v.Mod, depth + 1, min);
            }

            return min.Value;
        }

        private static void ThirdWalk(DrawTreeNode tree, double n)
        {
            tree.X += n;
            foreach (var child in tree.Children)
            {
                ThirdWalk(child, n);
            }
        }

        public static DrawTreeNode CalculateLayout(TreeInfo rootInfo)
        {
            var tree = new DrawTreeNode(rootInfo);
            var dt = Firstwalk(tree);
            var min = SecondWalk(dt);
            if (min < 0)
            {
                ThirdWalk(dt, -min);
            }
            return dt;
        }

        // 可选：添加获取节点位置的辅助方法
        public static Vector2 GetNodePosition(DrawTreeNode node)
        {
            return new Vector2((float)node.X, (float)node.Y);
        }
        public static Dictionary<TreeInfo, Vector2Int> GetNodePositions(
        TreeInfo rootInfo,
        int horizontalSpacing = 100,
        int verticalSpacing = 100)
        {
            var positions = new Dictionary<TreeInfo, Vector2Int>();
            var root = CalculateLayout(rootInfo);

            // 使用队列进行广度优先遍历，收集所有节点位置
            var queue = new Queue<DrawTreeNode>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                // 将浮点数坐标转换为整数坐标，并应用缩放因子
                var position = new Vector2Int(
                    Mathf.RoundToInt((float)node.X * horizontalSpacing),
                    Mathf.RoundToInt((float)node.Y * verticalSpacing)
                );

                positions[node.Info] = position;

                // 将子节点加入队列
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }

            MoveOffsetNodePostions(positions);
            return positions;
        }
        private static void MoveOffsetNodePostions(Dictionary<TreeInfo, Vector2Int> posMap)
        {
            var posxArr = posMap.Values.Select(p => p.x).ToArray();
            var max = Mathf.Max(posxArr);
            var min = Mathf.Min(posxArr);
            var offset = Mathf.FloorToInt((max - min) * 0.5f);
            foreach (var node in posMap.Keys.ToArray())
            {
                var pos = posMap[node];
                pos.x -= offset;
                posMap[node] = pos;
            }
        }
    }
}
