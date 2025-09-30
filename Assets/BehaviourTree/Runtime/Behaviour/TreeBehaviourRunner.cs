using UnityEngine;

namespace UFrame.BehaviourTree
{
    public class TreeBehaviourRunner : BTreeRunnerBase
    {
        [SerializeField]
        protected NodeBehaviour _rootTask;

        protected override BTree CreateInstanceTree()
        {
            var tree = ScriptableObject.CreateInstance<BTree>();
            tree.rootTree = _rootTask?.CreateTreeInfo();
            return tree;
        }
    }
}
