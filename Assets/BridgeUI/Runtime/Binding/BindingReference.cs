/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定模板                                                                        *
*//************************************************************************************/

using System;
using UnityEngine;

namespace UFrame.BridgeUI
{
    public abstract class BindingReference : MonoBehaviour, IClassReference
    {
        [SerializeField]
        protected string m_viewTypeName;
        [SerializeField]
        protected string m_logicTypeName;
        [HideInInspector]
        public string m_scriptGuid;

        public string ViewTypeName => m_viewTypeName;
        public string LogicTypeName => m_logicTypeName;

        public Type LoadViewScriptType()
        {
            var viewScriptType = BridgeUI.Utility.FindTypeInAllAssemble(m_viewTypeName);
            if (viewScriptType != null)
            {
                return viewScriptType;
            }
            Debug.Log("未编写脚本：" + m_viewTypeName);
            return null;
        }

        public bool TryLoadLogicScriptType(out Type logicScriptType)
        {
            if (string.IsNullOrEmpty(m_logicTypeName))
            {
                logicScriptType = null;
                return false;
            }

            logicScriptType = BridgeUI.Utility.FindTypeInAllAssemble(m_logicTypeName);
            if (logicScriptType != null)
            {
                return true;
            }
            return false;
        }
    }

    //带参数的绑定
    public abstract class BindingReference<T> : BindingReference
    {
        [SerializeField]
        protected T m_parameter;
    }
}