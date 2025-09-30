/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定器                                                                          *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI
{
    public class RuntimeViewBase: ViewBase
    {
        protected SubView rootScript;

        protected override void OnBinding(GameObject target)
        {
            base.OnBinding(target);
            var reference = target.GetComponent<BindingReference>();

            if(reference != null)
            {
                var rootScriptType = reference.LoadViewScriptType();
                if (rootScriptType != null && typeof(SubView).IsAssignableFrom(rootScriptType))
                {
                    rootScript = System.Activator.CreateInstance(rootScriptType) as SubView;
                    rootScript.Binding(reference);
                    RegistUIControl(rootScript);
                }
            }
           
        }

        protected override void OnUnBinding()
        {
            base.OnUnBinding();
            if (rootScript != null)
            {
                rootScript.UnBinding();
                RemoveUIControl(rootScript);
            }
        }
    }
}