/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 对外接口                                                                        *
*//************************************************************************************/
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    /// <summary>
    /// 界面操作接口
    /// </summary>
    public class SingleUIFacade : IUIFacade
    {
        private UIHandlePool handlePool = new UIHandlePool();
        private event UnityAction<IUIPanel> onCreate;
        private event UnityAction<IUIPanel> onClose;
        private event UnityAction<IUIPanel,bool> onHide;
        protected IPanelGroup m_panelGroup;
        public HashSet<string> LoadingPanels { get; private set; } = new HashSet<string>();
        protected HashSet<IPanelGroup> m_panelGroups = new HashSet<IPanelGroup>();
        public HashSet<IUIPanel> OpendPanels => m_openedPanels;
        protected HashSet<IUIPanel> m_openedPanels = new HashSet<IUIPanel>();

        public void Open(string panelName, object data = null)
        {
            this.Open(null, panelName, data);
        }

        public void Open(IUIPanel parentPanel, string panelName, object data = null)
        {
            this.Open(parentPanel, panelName, -1, data);
        }

        public void Open(IUIPanel parentPanel, string panelName, int index, object data = null)
        {
            var handle = handlePool.Allocate(panelName);
            OpenInternal(handle, parentPanel, panelName, index, data);
        }

        public void Open(string panelName, IPanelVisitor uiData)
        {
            Open(null, panelName, uiData);
        }

        public void Open(IUIPanel parentPanel, string panelName, IPanelVisitor uiData)
        {
            var handle = handlePool.Allocate(panelName);
            object data = null;
            if (uiData != null)
            {
                data = uiData.Data;
                uiData.Binding(handle);
            }
            OpenInternal(handle, parentPanel, panelName, -1, data);
        }

        protected virtual void OpenInternal(UIHandle handle, IUIPanel parent, string panelName, int index, object data = null)
        {
            LoadingPanels.Add(panelName);
            handle.RegistCreate(OnCreate);
            handle.RegistClose(OnClose);
            handle.RegistToggleHide(OnHide);

            var currentGroup = parent == null ? null : parent.Group;
            var openOK = false;
            if (currentGroup == null)//限制性打开
                currentGroup = m_panelGroup;

            if (currentGroup != null)
            {
                var bridge = currentGroup.FindOrCreateBridge(parent, panelName, index);
                if (bridge != null)
                {
                    handle.RegistBridge(bridge);
                    currentGroup.OpenPanelWithBridge(bridge);
                    openOK = true;
                }
            }
            if (openOK)
            {
                if (data != null)
                {
                    handle.Send(data);
                }
            }
            else
            {
                Debug.Log("SingleUIFacade failed open:" + panelName);
            }
        }

        public void Hide(string panelName)
        {
            Hide(null, panelName);
        }

        public virtual void Hide(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup == null)
                currentGroup = m_panelGroup;
            if (currentGroup != null)
            {
                currentGroup.HidePanel(panelName);
            }
            LoadingPanels.Remove(panelName);
        }

        public virtual void UnHide(string panelName)
        {
            UnHide(null, panelName);
        }

        public virtual void UnHide(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup == null)
                currentGroup = m_panelGroup;
            if (currentGroup != null)
            {
                currentGroup.UnHidePanel(panelName);
            }
        }

        public virtual void Close(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup == null)
                currentGroup = m_panelGroup;
            if (currentGroup != null)
            {
                currentGroup.ClosePanel(panelName);
            }
            LoadingPanels.Remove(panelName);
        }

        public void Close(string panelName)
        {
            Close(null, panelName);
        }

        public bool IsPanelOpen(string panelName,bool includeHide = false)
        {
            if (m_panelGroup != null)
            {
                return m_panelGroup.IsPanelOpen(panelName, includeHide);
            }
            return false;
        }

        public bool IsPanelOpen<T>(string panelName, out T[] panels,bool includeHide = false) where T:IUIPanel
        {
            panels = null;
            if (m_panelGroup != null)
            {
                if (m_panelGroup.IsPanelOpen(panelName, out T[] subpanels, includeHide))
                {
                    panels = subpanels;
                }
            }
            return panels != null;
        }


        public void RegistCreate(UnityAction<IUIPanel> onCreate)
        {
            if (onCreate == null) return;
            this.onCreate -= onCreate;
            this.onCreate += onCreate;
        }

        public void RegistClose(UnityAction<IUIPanel> onClose)
        {
            if (onClose == null) return;
            this.onClose -= onClose;
            this.onClose += onClose;
        }
        public void RemoveClose(UnityAction<IUIPanel> onClose)
        {
            if (onClose == null) return;
            this.onClose -= onClose;
        }
        public void RegistHide(UnityAction<IUIPanel,bool> onHide)
        {
            if (onHide == null) return;
            this.onHide -= onHide;
            this.onHide += onHide;
        }
        public void RemoveHide(UnityAction<IUIPanel, bool> onHide)
        {
            if (onHide == null) return;
            this.onHide -= onHide;
        }

        public void RemoveCreate(UnityAction<IUIPanel> onCreate)
        {
            if (onCreate == null) return;
            this.onCreate -= onCreate;
        }

        protected virtual void OnCreate(IUIPanel panel)
        {
            if (this.onCreate != null)
            {
                this.onCreate.Invoke(panel);
            }
            LoadingPanels.Remove(panel.Name);
            m_openedPanels.Add(panel);
        }

        protected virtual void OnClose(IUIPanel panel)
        {
            if (this.onClose != null)
            {
                this.onClose.Invoke(panel);
            }
            m_openedPanels?.Remove(panel);
        }


        protected virtual void OnHide(IUIPanel panel, bool hide)
        {
            if (this.onHide != null)
            {
                this.onHide.Invoke(panel, hide);
            }
            if (hide)
            {
                m_openedPanels.Remove(panel);
            }
            else
            {
                m_openedPanels.Add(panel);
            }
        }

        public void RegistGroup(IPanelGroup group)
        {
            m_panelGroup = group;
        }

        public void RemoveGroup(IPanelGroup group)
        {
            if (group == m_panelGroup)
                group = null;
        }

        public virtual bool Exist(string panelName)
        {
            if(m_panelGroup != null)
            {
                return m_panelGroup.Nodes.ContainsKey(panelName);
            }
            return false;
        }

        public void Send(string panelName, object data)
        {
            if (m_panelGroup != null)
            {
                m_panelGroup.Send(panelName, data);
            }
        }
    }
}
