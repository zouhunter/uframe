/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 多线程界面接口                                                                  *
*//************************************************************************************/

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    public class UIThreadFacade : MonoBehaviour
    {
        private MutiUIFacade uiFacade;
        private Queue<Action> mainThreadActions = new Queue<Action>();
        public static UIThreadFacade Instance { get; private set; }

        private void Awake()
        {
            uiFacade = new MutiUIFacade();
            Instance = this;
        }
        private void Update()
        {
            if(mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action.Invoke();
            }
        }
        public void Close(string panelName)
        {
            mainThreadActions.Enqueue(new Action(()=> {
                uiFacade.Close(panelName);
            }));
        }

        public void Close(IPanelGroup parentGroup, string panelName)
        {
            mainThreadActions.Enqueue(new Action(() => {
                uiFacade.Close(parentGroup,panelName);
            }));
        }

        public void Hide(string panelName)
        {
            mainThreadActions.Enqueue(new Action(() => {
                uiFacade.Hide(panelName);
            }));
        }

        public void Hide(IPanelGroup parentGroup, string panelName)
        {
            mainThreadActions.Enqueue(new Action(() => {
                uiFacade.Hide(parentGroup,panelName);
            }));
        }

        public void IsPanelOpen(string panelName, UnityAction<bool> onJudge = null)
        {
            mainThreadActions.Enqueue(new Action(() => {
                var isOpen = uiFacade.IsPanelOpen(panelName);
                if (onJudge != null)
                {
                    onJudge.Invoke(isOpen);
                }
            }));
        }

        public void IsPanelOpen(IPanelGroup parentGroup, string panelName,UnityAction<bool> onJudge = null)
        {
            mainThreadActions.Enqueue(new Action(() => {
                var isOpen = uiFacade.IsPanelOpen(panelName);
                if (onJudge != null)
                {
                    onJudge.Invoke(isOpen);
                }
            }));
        }

        public void Open(string panelName, object data = null)
        {
            mainThreadActions.Enqueue(new Action(() =>
            {
                uiFacade.Open(panelName, data);
            }));
        }

        public void Open(IUIPanel parentPanel, string panelName, object data = null)
        {
            mainThreadActions.Enqueue(new Action(() =>
            {
                uiFacade.Open(parentPanel, panelName, data);
            }));
        }

        public void RegistCreate(UnityAction<IUIPanel> onCreate)
        {
            uiFacade.RegistCreate(onCreate);
        }

        public void RemoveCreate(UnityAction<IUIPanel> onCreate)
        {
            uiFacade.RemoveCreate(onCreate);
        }

        public void RegistClose(UnityAction<IUIPanel> onClose)
        {
            uiFacade.RegistClose(onClose);
        }

        public void RemoveClose(UnityAction<IUIPanel> onClose)
        {
            uiFacade.RemoveClose(onClose);
        }

        public void RegistHide(UnityAction<IUIPanel,bool> onHide)
        {
            uiFacade.RegistHide(onHide);
        }

        public void RemoveHide(UnityAction<IUIPanel, bool> onHide)
        {
            uiFacade.RemoveHide(onHide);
        }
    }
}
