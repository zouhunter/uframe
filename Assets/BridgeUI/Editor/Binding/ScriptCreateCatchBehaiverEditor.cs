using UnityEngine;
using UnityEditor;

namespace UFrame.BridgeUI.Editors
{ 
    [CustomEditor(typeof(ScriptCreateCatchBehaiver))]
    public class ScriptCreateCatchBehaiverEditor:Editor
    {
        private void OnEnable()
        {
            ScriptCreateCatchBehaiver targetBehavier = target as ScriptCreateCatchBehaiver;
            if(targetBehavier)
            {
                Component componetAdd = null;
                var assemble = System.Reflection.Assembly.Load(targetBehavier.assembleCatch);
                if(assemble != null)
                {
                    var type = assemble.GetType(targetBehavier.typeCatch);
                    if (type != null)
                    {
                        componetAdd = Selection.activeGameObject.GetComponent(type);
                        if(componetAdd == null)
                        {
                            componetAdd = Selection.activeGameObject.AddComponent(type);
                        }
                        GameObject.DestroyImmediate(targetBehavier, true);
                        Debug.Log("Type Find Ok:" + targetBehavier.typeCatch);
                    }
                }
                if(componetAdd == null)
                {
                    Debug.Log("Type not Exists:"+ targetBehavier.typeCatch);
                }
            }
        }
    }
}