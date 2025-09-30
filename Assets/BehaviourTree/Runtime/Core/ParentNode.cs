using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public abstract class ParentNode : BaseNode
    {
        public virtual int maxChildCount { get => int.MaxValue; }

        protected virtual void OnStart(TreeInfo info)
        {
            info.subIndex = 0;
        }
        protected virtual byte OnUpdate(TreeInfo info)
        {
            return Status.Inactive;
        }
        protected virtual void OnEnd(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees.Count > 0)
            {
                for (int i = 0; i < info.subTrees.Count; i++)
                {
                    var child = info.subTrees[i];
                    if (!child.enable || child.node == null)
                        continue;

                    if (child.status == Status.Running)
                    {
                        child.node.DoEnd(child);
                        child.status = Status.Failure;
                    }
                }
            }
            base.OnEnd(info);
        }
        protected override byte OnUpdate(ITreeInfo info)
        {
            return OnUpdate(info as TreeInfo);
        }
        protected override void OnStart(ITreeInfo info)
        {
            OnStart(info as TreeInfo);
        }
        protected override void OnEnd(ITreeInfo info)
        {
            OnEnd(info as TreeInfo);
        }
    }
}
