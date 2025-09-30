using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BridgeUI.Editors
{
    //[System.Serializable]
    public class ComponentItem
    {
        public bool open = false;
        public string name;
        public int componentID;
        public TypeInfo[] components;
        public List<BindingShow> viewItems = new List<BindingShow>();
        public List<BindingEvent> eventItems = new List<BindingEvent>();
        public GameObject target;
        public ScriptableObject scriptTarget;
        public bool isScriptComponent;
        public string[] componentStrs { get { return System.Array.ConvertAll<TypeInfo, string>(components, x => x.typeName); } }
        public System.Type componentType
        {
            get
            {
                var type = typeof(GameObject);
                if (components != null && components.Length > componentID && componentID >= 0)
                {
                    type = components[componentID].type;
                }
                return type;
            }
        }
        public bool IsIUIControl
        {
            get
            {
                if (componentType == null) return false;

                if (typeof(BridgeUI.IUIControl).IsAssignableFrom(componentType))
                {
                    return true;
                }
                Debug.Log(componentType);
                return false;
            }
        }
        public ComponentItem() {
        }

        public ComponentItem(GameObject target)
        {
            this.name = target.name;
            this.target = target;
        }
        
        public ComponentItem(ScriptableObject target)
        {
            UpdateAsScriptObject(target);
        }

        public void UpdateAsScriptObject(ScriptableObject target)
        {
            this.name = target.name;
            this.scriptTarget = target;
            this.isScriptComponent = true;
            this.componentID = 0;
            this.components = new TypeInfo[] { new TypeInfo(target.GetType()) };
        }
    }


}