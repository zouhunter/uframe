using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UFrame.BridgeUI
{
    public class UIBindingItem
    {
        public Action<IUIPanel, object> openAction { get; set; }
        public Action closeAction { get; set; }
        public Action hideAction { get; set; }
        public Func<bool> isOpenAction { get; set; }
    }

    public class UIBindingController
    {
        private Dictionary<string, Dictionary<int, UIBindingItem>> panelEventDic = new Dictionary<string, Dictionary<int, UIBindingItem>>();
        public void RegistPanelEvent(string parentPanel, int key, UIBindingItem onOpen)
        {
            if (onOpen != null && !string.IsNullOrEmpty(parentPanel))
            {
                if (!panelEventDic.ContainsKey(parentPanel))
                {
                    panelEventDic[parentPanel] = new Dictionary<int, UIBindingItem>();
                }
                panelEventDic[parentPanel][key] = onOpen;
            }
        }
        public void RemovePanelEvent(string parentPanel, int key, UIBindingItem onOpen)
        {
            if (onOpen != null && panelEventDic != null && !string.IsNullOrEmpty(parentPanel))
            {
                if (panelEventDic.ContainsKey(parentPanel) && panelEventDic[parentPanel].ContainsKey(key) && panelEventDic[parentPanel][key] != null)
                {
                    panelEventDic[parentPanel].Remove(key);
                }
            }
        }

        public bool IsRegistedPanelOpen(IUIPanel panel, int key)
        {
            var bindingItem = FindBindingItem(panel.Name, key);
            if(bindingItem != null)
            {
                return bindingItem.isOpenAction.Invoke();
            }
            return false;
        }

        public void OpenRegistedPanel(IUIPanel panel, int key, object data = null)
        {
            var bindingItem = FindBindingItem(panel.Name, key);
            if (bindingItem!= null && bindingItem.openAction != null){
                bindingItem.openAction.Invoke(panel, data);
            }
        }

        public void HideRegistedPanel(IUIPanel panel, int key)
        {
            var bindingItem = FindBindingItem(panel.Name, key);
            if (bindingItem != null && bindingItem.hideAction != null)
            {
                bindingItem.hideAction.Invoke();
            }
        }


        public void CloseRegistedPanel(IUIPanel panel, int key)
        {
            var bindingItem = FindBindingItem(panel.Name, key);
            if (bindingItem != null && bindingItem.closeAction != null)
            {
                bindingItem.closeAction.Invoke();
            }
        }

        public  UIBindingItem FindBindingItem(string panelName, int key)
        {
            var parentName = panelName;////panel的名字不能改变
            if (panelEventDic.ContainsKey(parentName) && 
                panelEventDic[parentName] != null && 
                panelEventDic[parentName].ContainsKey(key) && 
                panelEventDic[parentName][key] != null)
            {
                return panelEventDic[parentName][key];
            }
            return null;
        }
    }
}