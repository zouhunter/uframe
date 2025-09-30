using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BridgeUI.Graph
{
    public class SingleRuntimeUIFacade : SingleUIFacade
    {
        protected Transform m_context;
        public RuntimePanelGroup BindUIParent(Transform context,UILoader bundleLoader = null)
        {
            var group = context.GetComponent<RuntimePanelGroup>();
            if (group == null)
            {
                group = context.gameObject.AddComponent<RuntimePanelGroup>();
            }
            if (bundleLoader != null)
            {
                group.bundleCreateRule = bundleLoader;
            }
            RegistGroup(group);
            return group;
        }
        public void RegistUIData(UIGraph uiData)
        {
            if(m_panelGroup == null || !(m_panelGroup is RuntimePanelGroup))
            {
                Debug.LogError("empty runtime panel group!");
                return;
            }
            (m_panelGroup as RuntimePanelGroup).RegistUIData(uiData);
        }
    }
}