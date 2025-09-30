using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.BridgeUI;

using UnityEngine;
using UnityEngine.Scripting;

namespace UFrame.BridgeUI
{
    public class ViewBaseComponent : MonoBehaviour, IDataReceiver, ICustomUI, IDiffuseView
    {
        protected IUIPanel view;
        protected bool Initialized;

        public virtual Transform Content
        {
            get
            {
                return null;
            }
        }

        protected virtual void Start()
        {
            if (view == null)
            {
                var panel = new ViewBase_Diffuse();
                panel.Binding(gameObject);
                Debug.LogWarning("ViewBaseComponent Start: view == null,use diffuse!");
            }
        }

        public void Initialize(IUIPanel panel)
        {
            if (!Initialized)
            {
                this.view = panel;
                Initialized = true;
                OnInitialize();
            }
            else
            {
                Debug.LogError("ViewBaseComponent Already Initialized, Please Check Logic!");
            }
        }

        public void Recover()
        {
            if (Initialized)
            {
                Initialized = false;
                OnRecover();
                view = null;
            }
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnRecover() { }

        protected virtual void OnMessageReceive(object message)
        {
        }

        public void HandleData(object data)
        {
            if (Initialized)
            {
                OnMessageReceive(data);
            }
        }

        public virtual void Close()
        {
            if (Initialized)
            {
                view.Close();
            }
        }

        public virtual void CallBack(object data)
        {
            if (Initialized)
            {
                view.CallBack(data);
            }
        }

        public virtual void Close(int index)
        {
            if (Initialized)
            {
                view.Close(index);
            }
        }

        public virtual void Open(int panelName, object data)
        {
            if (Initialized)
            {
                view.Open(panelName, data);
            }
        }

        public virtual void Open(string panelName, object data)
        {
            if (Initialized)
            {
                view.Open(panelName, data);
            }
        }

        public virtual void Hide(string panelName)
        {
            if (Initialized)
            {
                view.Hide(panelName);
            }
        }

        public virtual void Hide(int index)
        {
            if (Initialized)
            {
                view.Hide(index);
            }
        }

        public virtual void Close(string panelName)
        {
            if (Initialized)
            {
                view.Close(panelName);
            }
        }

        public virtual bool IsOpen(int index)
        {
            if (Initialized)
            {
                return view.IsOpen(index);
            }
            return false;
        }
        public virtual bool IsOpen(string panelName)
        {
            if (Initialized)
            {
                return view.IsOpen(panelName);
            }
            return false;
        }
    }
}