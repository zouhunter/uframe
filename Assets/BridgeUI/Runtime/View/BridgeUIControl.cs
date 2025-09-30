/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 控件模板                                                                    *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI
{
    public abstract class BridgeUIControl : MonoBehaviour, IUIControl
    {
        protected IUIPanel context;
        private bool initialized;
        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }
        public void Initialize(IUIPanel context = null)
        {
            if (!initialized)
            {
                this.context = context;
                initialized = true;
                OnInitialize();
            }
        }
        public void Release()
        {
            if (initialized)
            {
                OnRelease();
                initialized = false;
                context = null;
            }
        }
        protected virtual void OnInitialize() { }
        protected virtual void OnRelease() { }
    }
}