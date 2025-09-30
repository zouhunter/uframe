/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 装饰器
 *_*/
namespace UFrame.BehaviourTree
{
    /// <summary>
    /// 装饰器
    /// </summary>
    public abstract class DecoratorNode : ParentNode
    {
        public override int maxChildCount => 1;

        protected virtual byte ExecuteChild(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees.Count > 0)
            {
                var childNode = info.subTrees[0];
                if (childNode != null && childNode.enable && childNode.node != null)
                {
                    return childNode.node.Execute(childNode);
                }
            }
            return Status.Inactive;

        }
    }
}
