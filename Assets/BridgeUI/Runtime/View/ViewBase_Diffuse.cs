using System;
using UnityEngine;

namespace UFrame.BridgeUI
{
    public class ViewBase_Diffuse : ViewBase
    {
        private ICustomUI component;
        public override Transform Content
        {
            get
            {
                if (component == null || component.Content == null)
                    return base.Content;
                return component.Content;
            }
        }

        protected override void OnBinding(GameObject target)
        {
            base.OnBinding(target);
            component = target.GetComponent<ICustomUI>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (component != null)
            {
                if (component is BridgeUI.IDataReceiver)
                {
                    RegistOnRecevie((component as BridgeUI.IDataReceiver).HandleData);
                }
                component.Initialize(this);
            }
            else
            {
                Debug.Log("ViewBase_Diffuse component == null!");
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            if (component != null)
            {
                if (component is BridgeUI.IDataReceiver)
                {
                    RemoveOnRecevie((component as BridgeUI.IDataReceiver).HandleData);
                }
                component.Recover();
            }
        }
    }

}
