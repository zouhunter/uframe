using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Decision
{
    [System.Serializable]
    public class DecisionLeafNode : DecisionTreeNode
    {
        [SerializeReference]
        public DecisionResult result;
        public override DecisionResult Result => result;
    }
}
