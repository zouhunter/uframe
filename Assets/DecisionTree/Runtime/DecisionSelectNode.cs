using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Decision
{
    [System.Serializable]
    public class DecisionSelectNode : DecisionTreeNode
    {
        public string question;
        [SerializeReference]
        public List<DecisionTreeNode> childs = new List<DecisionTreeNode>();

        public override List<DecisionTreeNode> Children => childs;
        public override string Question
        {
            get
            {
                return question;
            }
            set
            {
                question = value;
            }
        }
    }
}
