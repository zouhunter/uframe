/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 对外接口                                                                        *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    /// <summary>
    /// 界面操作接口
    /// </summary>
    public class MutiUIFacade : IUIFacade
    {
        private UIHandlePool handlePool = new UIHandlePool();
        private event UnityAction<IUIPanel> onCreate;
        private event UnityAction<IUIPanel> onClose;
        private event UnityAction<IUIPanel, bool> onHide;
        protected HashSet<IPanelGroup> m_panelGroups = new HashSet<IPanelGroup>();
        public HashSet<string> LoadingPanels { get; private set; } = new HashSet<string>();
        public HashSet<IUIPanel> OpendPanels => m_openedPanels;
        protected HashSet<IUIPanel> m_openedPanels = new HashSet<IUIPanel>();

        public virtual void Open(string panelName, object data = null)
        {
            this.Open(null, panelName, data);
        }

        public virtual void Open(IUIPanel parentPanel, string panelName, object data = null)
        {
            this.Open(parentPanel, panelName, -1, data);
        }

        public virtual void Open(IUIPanel parentPanel, string panelName, int index, object data = null)
        {
            var handle = handlePool.Allocate(panelName);
            OpenInternal(handle, parentPanel, panelName, index, data);
        }


        public virtual void Open(string panelName, IPanelVisitor uiData)
        {
            Open(null, panelName, uiData);
        }

        public virtual void Open(IUIPanel parentPanel, string panelName, IPanelVisitor uiData)
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
            if (currentGroup != null)//限制性打开
            {
                var bridge = currentGroup.FindOrCreateBridge(parent, panelName, index);
                if (bridge != null)
                {
                    handle.RegistBridge(bridge);
                    currentGroup.OpenPanelWithBridge(bridge);
                    openOK = true;
                }
            }
            else
            {
                foreach (var group in m_panelGroups)
                {
                    var bridge = group.FindOrCreateBridge(parent, panelName, index);

                    if (bridge != null)
                    {
                        handle.RegistBridge(bridge);
                        group.OpenPanelWithBridge(bridge);
                        openOK = true;
                    }
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
                Debug.Log("MutiUIFacade failed open:" + panelName);
            }
        }

        public virtual void Hide(string panelName)
        {
            Hide(null, panelName);
        }

        public virtual void Hide(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup != null)//限制性打开
            {
                currentGroup.HidePanel(panelName);
            }
            else
            {
                foreach (var group in m_panelGroups)
                {
                    group.HidePanel(panelName);
                }
            }
            LoadingPanels.Remove(panelName);
        }

        public virtual void UnHide(string panelName)
        {
            UnHide(null, panelName);
        }

        public virtual void UnHide(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup != null)//限制性打开
            {
                currentGroup.UnHidePanel(panelName);
            }
            else
            {
                foreach (var group in m_panelGroups)
                {
                    group.UnHidePanel(panelName);
                }
            }
        }


        public virtual void Close(IPanelGroup currentGroup, string panelName)
        {
            if (currentGroup != null)
            {
                currentGroup.ClosePanel(panelName);
            }
            else
            {
                foreach (var group in m_panelGroups)
                {
                    group.ClosePanel(panelName);
                }
            }
            LoadingPanels.Remove(panelName);
        }

        public virtual void Close(string panelName)
        {
            Close(null, panelName);
        }

        public virtual bool IsPanelOpen(string panelName,bool includeHide = false)
        {
            bool globleHave = false;

            foreach (var group in m_panelGroups)
            {
                globleHave |= group.IsPanelOpen(panelName, includeHide);
            }
            return globleHave;
        }
        
        public virtual bool IsPanelOpen<T>(string panelName, out T[] panels,bool includeHide = false) where T:IUIPanel
        {
            var objpanels = new List<T>();
            var findPanel = false;
            foreach (var group in m_panelGroups)
            {
                T[] subpanels = null;
                if (group.IsPanelOpen(panelName, out subpanels,false))
                {
                    objpanels.AddRange(subpanels);
                    findPanel = true;
                }
            }

            if (findPanel)
            {
                panels = objpanels.ToArray();
            }
            else
            {
                panels = null;
            }

            return findPanel;
        }


        public virtual void RegistCreate(UnityAction<IUIPanel> onCreate)
        {
            if (onCreate == null) return;
            this.onCreate -= onCreate;
            this.onCreate += onCreate;
        }
        public virtual void RemoveCreate(UnityAction<IUIPanel> onCreate)
        {
            if (onCreate == null) return;
            this.onCreate -= onCreate;
        }
        public virtual void RegistClose(UnityAction<IUIPanel> onClose)
        {
            if (onClose == null) return;
            this.onClose -= onClose;
            this.onClose += onClose;
        }
        public virtual void RemoveClose(UnityAction<IUIPanel> onClose)
        {
            if (onClose == null) return;
            this.onClose -= onClose;
        }
        public virtual void RegistHide(UnityAction<IUIPanel, bool> onHide)
        {
            if (onHide == null) return;
            this.onHide -= onHide;
            this.onHide += onHide;
        }
        public virtual void RemoveHide(UnityAction<IUIPanel, bool> onHide)
        {
            if (onHide == null) return;
            this.onHide -= onHide;
        }

        protected virtual void OnCreate(IUIPanel panel)
        {
            LoadingPanels.Remove(panel.Name);
            m_openedPanels.Add(panel);
            if (this.onCreate != null)
            {
                this.onCreate.Invoke(panel);
            }
        }

        protected virtual void OnClose(IUIPanel panel)
        {
            m_openedPanels?.Remove(panel);
            if (this.onClose != null)
            {
                this.onClose.Invoke(panel);
            }
        }


        protected virtual void OnHide(IUIPanel panel,bool hide)
        {
            if (hide)
            {
                m_openedPanels.Remove(panel);
            }
            else
            {
                m_openedPanels.Add(panel);
            }
            if (this.onHide != null)
            {
                this.onHide.Invoke(panel, hide);
            }
        }

        public void RegistGroup(IPanelGroup group)
        {
            if (group != null)
                m_panelGroups.Add(group);
        }

        public void RemoveGroup(IPanelGroup group)
        {
            if (group != null)
                m_panelGroups.Remove(group);
        }

        public bool Exist(string panelName)
        {
            foreach (var group in m_panelGroups)
            {
                if (group.Nodes.ContainsKey(panelName))
                    return true;
            }
            return false;
        }

        public void Send(string panelName, object data)
        {
            foreach (var group in m_panelGroups)
            {
                if(group.Nodes.ContainsKey(panelName))
                {
                    group.Send(panelName, data);
                }
            }
        }

        public bool ExistLayer(UILayerType layer)
        {
            foreach (var panel in m_openedPanels)
            {
                if (panel.UType.layer == layer)
                    return true;
            }
            return false;
        }
    }
}