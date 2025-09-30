using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BehaviourTree
{
    public abstract class ActionNode : BaseNode
    {
        protected override byte OnUpdate(ITreeInfo info)
        {
            var result = OnUpdate();
            PostExecute(info, result);
            return result;
        }
        protected virtual void PostExecute(ITreeInfo info, byte status)
        {
            if (status != Status.Success)
                return;

            if (info is TreeInfo treeInfo && treeInfo.subTrees != null && treeInfo.subTrees.Count > 0)
            {
                foreach (var subTree in treeInfo.subTrees)
                {
                    if (subTree.enable)
                    {
                        try
                        {
                            subTree.node.Execute(subTree);
                        }
                        catch (System.Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                    }
                }
            }
        }
    }
}
