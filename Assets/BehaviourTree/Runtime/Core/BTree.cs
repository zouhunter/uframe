using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BehaviourTree
{
    [System.Serializable]
    public class BTree : OwnerBase, IOwner
    {
        [SerializeField]
        protected TreeInfo _rootTree;
        public virtual TreeInfo rootTree
        {
            get { return _rootTree; }
            set { _rootTree = value; }
        }
        public bool TreeStarted => _treeStarted;
        private bool _treeStarted;
        private IOwner _ownerTree;
        public IOwner Owner
        {
            get
            {
                return _ownerTree ?? this;
            }
        }
        #region TreeTraversal
        public BTree CreateInstance()
        {
            return Instantiate(this);
        }

        public void CollectToDic(TreeInfo info, Dictionary<string, TreeInfo> infoDic)
        {
            if (info == null || info.id == null)
                return;

            infoDic[info.id] = info;
            info.subTrees?.ForEach(t =>
            {
                CollectToDic(t, infoDic);
            });
        }

        public virtual TreeInfo FindTreeInfo(string id, bool deep = true)
        {
            var dic = new Dictionary<string, TreeInfo>();
            CollectToDic(rootTree, dic);
            if (dic.TryGetValue(id, out TreeInfo treeInfo))
            {
                return treeInfo;
            }
            return null;
        }

        public virtual bool StartUp()
        {
            if (rootTree != null && rootTree.node != null)
            {
                TickCount = 0;
                _ownerTree = this;
                TreeInfoUtil.SetOwnerDeepth(rootTree, this);
                _treeStarted = true;
                return true;
            }
            Debug.LogError("rootTree empty!" + (rootTree == null));
            return false;
        }

        public void Stop()
        {
            _treeStarted = false;
            CleanDeepth(rootTree);
        }

        public void SetOwner(IOwner owner)
        {
            _ownerTree = owner;
            TreeInfoUtil.SetOwnerDeepth(rootTree, owner);
        }

        public bool ResetStart()
        {
            if (rootTree != null && _treeStarted)
            {
                TickCount = 0;
                TreeInfoUtil.SetOwnerDeepth(rootTree, this);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 深度清理
        /// </summary>
        /// <param name="info"></param>
        public void CleanDeepth(TreeInfo info)
        {
            if (info.subTrees != null && info.subTrees != null)
            {
                foreach (var subInfo in info.subTrees)
                {
                    if (subInfo.enable)
                    {
                        CleanDeepth(subInfo);
                    }
                }
            }
            if (info.condition.enable && info.condition.conditions != null && info.node == this)
            {
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.subConditions != null)
                    {
                        for (int i = 0; i < condition.subConditions.Count; i++)
                        {
                            var condition2 = condition.subConditions[i];
                            condition2.node?.Clean();
                        }
                    }
                    condition.node?.Clean();
                }
            }
            info.node?.Clean();
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public virtual byte Tick()
        {
            if (rootTree == null || rootTree.node == null || rootTree.enable == false)
            {
                if (rootTree == null)
                    Debug.LogError("BTree rootTree == null" + name);
                if (rootTree.node == null)
                    Debug.LogError("BTree rootTree.node == null" + name);
                if (!rootTree.enable)
                    Debug.LogError("BTree rootTree.enable == false" + name);
                return Status.Inactive;
            }
            TickCount++;
            rootTree.node.Execute(rootTree);
            return rootTree.status;
        }

        internal void OnReset()
        {
            if (rootTree != null && rootTree.node != null)
            {
                TickCount = 0;
                TreeInfoUtil.SetOwnerDeepth(rootTree, Owner ?? this);
            }
        }
        #endregion
        /// <summary>
        /// 收集节点
        /// </summary>
        /// <param name="allNodes"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void CollectNodesDeepth(TreeInfo info, List<BaseNode> nodes)
        {
            if (info.node && !nodes.Contains(info.node))
            {
                nodes.Add(info.node);
            }
            if (info.condition != null && info.condition.conditions != null)
            {
                int i = 0;
                foreach (var condition in info.condition.conditions)
                {
                    if (condition.node && !nodes.Contains(condition.node))
                    {
                        nodes.Add(condition.node);
                    }

                    if (condition.subConditions != null)
                    {
                        int j = 0;
                        foreach (var subNode in condition.subConditions)
                        {
                            if (subNode != null && subNode.node && !nodes.Contains(subNode.node))
                            {
                                nodes.Add(subNode.node);
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            if (info.subTrees != null)
            {
                for (int i = 0; i < info.subTrees.Count; i++)
                {
                    var item = info.subTrees[i];
                    CollectNodesDeepth(item, nodes);
                }
            }
        }
    }
}
