/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 子界面绑定                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    [PanelParent]
    public abstract class SubView : BridgeUI.IUIControl
    {
        private IUIPanel context;
        private bool initialized;

        public IUIPanel Context { get { return context; } }
        public bool Initialized { get { return initialized; } }

        private List<IUIControl> subControls;

        public void Initialize(IUIPanel context = null)
        {
            if (!initialized)
            {
                this.context = context;
                OnInitialize();
                if (subControls != null)
                {
                    using (var enumeraotr = subControls.GetEnumerator())
                    {
                        while (enumeraotr.MoveNext())
                        {
                            if (!enumeraotr.Current.Initialized)
                                enumeraotr.Current.Initialize(Context);
                        }
                    }
                }
                initialized = true;
            }
        }

        public void Release()
        {
            if (initialized)
            {
                if (subControls != null)
                {
                    using (var enumeraotr = subControls.GetEnumerator())
                    {
                        while (enumeraotr.MoveNext())
                        {
                            enumeraotr.Current.Release();
                        }
                    }
                }
                OnRelease();
                initialized = false;
            }
        }
        protected virtual void OnInitialize() { }
        protected virtual void OnRelease() { }
        public abstract void Binding(BindingReference target);
        public abstract void UnBinding();

        /// <summary>
        /// 绑定子面板
        /// </summary>
        protected T BindingSubReference<T>(BindingReference subReference) where T : SubView, new()
        {
            var subPanel = new T();
            subPanel.Binding(subReference);
            RegistUIControl(subPanel);
            return subPanel;
        }

        /// <summary>
        /// 解绑定子面板
        /// </summary>
        /// <param name="subPanel"></param>
        protected void UnBindingSubReference(SubView subPanel)
        {
            if (subPanel != null)
            {
                subPanel.UnBinding();
                RemoveUIControl(subPanel);
            }
        }

        protected void RegistUIControl(IUIControl subPanel)
        {
            if (subPanel == null) return;

            if (subControls == null)
                subControls = new List<IUIControl>();

            if (!subControls.Contains(subPanel))
            {
                subControls.Add(subPanel);
                if (!subPanel.Initialized)
                    subPanel.Initialize(Context);
            }
        }

        protected void RemoveUIControl(IUIControl subPanel)
        {
            if (subPanel == null) return;

            if (subControls.Contains(subPanel))
            {
                subControls.Remove(subPanel);
            }
        }
    }
}