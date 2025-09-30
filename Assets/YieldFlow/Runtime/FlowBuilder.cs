//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2025-05-30
//* 描    述：
// 任务图构建器，支持链式API快速构建串行/并行依赖关系。
//* ************************************************************************************
using System.Collections.Generic;

namespace UFrame.YieldFlow
{
    /// <summary>
    /// 任务图构建器。支持链式API快速构建串行/并行依赖关系。
    /// 用于简化复杂任务流的依赖关系描述。
    /// </summary>
    public class FlowBuilder
    {
        /// <summary>
        /// 所有已注册的任务节点（去重）。
        /// </summary>
        private readonly List<FlowNode> _nodes = new();
        /// <summary>
        /// 上一次创建的任务节点集合（用于Then/Parallel依赖）。
        /// </summary>
        private readonly List<FlowNode> _lastCreated = new();
        /// <summary>
        /// 父级Builder（用于分支链式构建后回到主流程）
        /// </summary>
        private readonly FlowBuilder _parent;

        public FlowBuilder(FlowBuilder parent = null)
        {
            _parent = parent;
        }

        /// <summary>
        /// 添加一个新的任务节点作为起点。
        /// </summary>
        /// <param name="node">任务节点</param>
        /// <returns>链式调用自身</returns>
        public FlowBuilder Task(FlowNode node)
        {
            if (!_nodes.Contains(node))
                _nodes.Add(node);

            _lastCreated.Clear();
            _lastCreated.Add(node);
            return this;
        }

        /// <summary>
        /// 添加一个任务节点，并将其依赖于上一次创建的所有节点（串行）。
        /// </summary>
        /// <param name="next">下一个任务节点</param>
        /// <returns>链式调用自身</returns>
        public FlowBuilder Then(FlowNode next)
        {
            if (!_nodes.Contains(next))
                _nodes.Add(next);

            foreach (var prev in _lastCreated)
            {
                next.AddPredecessor(prev);
            }

            _lastCreated.Clear();
            _lastCreated.Add(next);
            return this;
        }
        /// <summary>
        /// 并行添加多个任务节点，所有节点依赖于上一次创建的节点。
        /// </summary>
        /// <param name="nodes">并行任务节点数组</param>
        /// <returns>链式调用自身</returns>
        public FlowBuilder Parallel(params FlowNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (!_nodes.Contains(node))
                    _nodes.Add(node);
                foreach (var prev in _lastCreated)
                {
                    node.AddPredecessor(prev);
                }
            }
            _lastCreated.Clear();
            _lastCreated.AddRange(nodes);
            return this;
        }

        /// <summary>
        /// 并行添加多个任务节点，所有节点依赖于上一次创建的节点。
        /// 返回每个分支的子Builder，可继续链式添加分支节点，最后可通过Return()回到主Builder。
        /// </summary>
        /// <param name="nodes">并行任务节点数组</param>
        /// <returns>每个分支的子Builder数组</returns>
        public FlowBuilder[] Parallels(params FlowNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (!_nodes.Contains(node))
                    _nodes.Add(node);
                foreach (var prev in _lastCreated)
                    node.AddPredecessor(prev);
            }
            _lastCreated.Clear();
            var branches = new FlowBuilder[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                branches[i] = new FlowBuilder(this).Task(nodes[i]);
            return branches;
        }

        /// <summary>
        /// 从分支Builder回到主Builder，继续主流程链式构建。
        /// </summary>
        /// <returns>父级Builder</returns>
        public FlowBuilder Return()
        {
            if (_parent == null)
                return this;
            foreach (var node in _nodes)
            {
                if (!_parent._nodes.Contains(node))
                    _parent._nodes.Add(node);
            }
            _parent._lastCreated.AddRange(_lastCreated);
            return _parent ?? this;
        }

        /// <summary>
        /// 条件分支：根据condition决定执行ifNode或elseNode（都可为null）。
        /// </summary>
        public FlowBuilder If(System.Func<bool> condition, FlowNode ifNode, FlowNode elseNode = null)
        {
            if (ifNode != null && !_nodes.Contains(ifNode))
                _nodes.Add(ifNode);
            if (elseNode != null && !_nodes.Contains(elseNode))
                _nodes.Add(elseNode);
            foreach (var prev in _lastCreated)
            {
                ifNode?.AddPredecessor(prev);
                elseNode?.AddPredecessor(prev);
            }
            _lastCreated.Clear();
            if (condition())
                _lastCreated.Add(ifNode);
            else if (elseNode != null)
                _lastCreated.Add(elseNode);
            return this;
        }

        /// <summary>
        /// 循环节点（伪实现，需配合LoopTaskNode等特殊节点使用）。
        /// </summary>
        public FlowBuilder While(System.Func<bool> condition, FlowNode loopNode)
        {
            if (!_nodes.Contains(loopNode))
                _nodes.Add(loopNode);
            foreach (var prev in _lastCreated)
                loopNode.AddPredecessor(prev);
            _lastCreated.Clear();
            _lastCreated.Add(loopNode);
            return this;
        }

        /// <summary>
        /// 设置_lastCreated中所有节点的前置依赖模式
        /// </summary>
        public FlowBuilder SetRelaxed()
        {
            foreach (var node in _lastCreated)
                node.SetAllPredecessorsRequired(false);
            return this;
        }

        /// <summary>
        /// 构建最终的任务节点列表（带依赖关系）。
        /// </summary>
        /// <returns>所有任务节点</returns>
        public List<FlowNode> Build() => _nodes;

        // 支持单独为某节点添加后续
        public void ThenFor(FlowNode from, FlowNode to)
        {
            if (!_nodes.Contains(to))
                _nodes.Add(to);
            to.AddPredecessor(from);
        }
    }
}

