using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI.Editors
{

    [System.Serializable]
    public class GenCodeRule
    {
        public int baseTypeIndex;
        public string nameSpace;
        public bool canInherit;
        public bool bindingAble;
        public UnityAction<Component> onGenerated;

        public GenCodeRule(string nameSpace)
        {
            this.nameSpace = nameSpace;
        }
    }
}