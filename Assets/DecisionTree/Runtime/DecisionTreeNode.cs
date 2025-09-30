using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Decision
{
    [System.Serializable]
    public abstract class DecisionTreeNode
    {
        [SerializeReference]
        public DecisionCondition condition;
        public int status;

        public virtual List<DecisionTreeNode> Children { get; }
        public virtual DecisionResult Result { get; }
        public virtual string Question { get; set; }

        public virtual void CopyTo(DecisionTreeNode treeInfo)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), treeInfo);
        }
    }
}
