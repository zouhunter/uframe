using UnityEngine;

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class NodeInfo
    {
        public UIType uiType = new UIType(new Color(.1f,.1f,.1f,.1f));
        public string guid;
        public string discription;
    }
}
