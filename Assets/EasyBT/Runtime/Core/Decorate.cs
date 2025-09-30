using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.EasyBT
{
    /// <summary>
    /// 装饰器
    /// </summary>
    public class Decorate : ParentNode
    {
        protected override int maxChildCount => 1;

        protected override NodeStatus OnUpdate()
        {
            if(_children.Count > 0)
            {
               return _children[0].Execute();
            }
            return NodeStatus.Inactive;
        }
    }
}
