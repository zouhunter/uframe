/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定模式的View                                                                  *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.BridgeUI
{
    [PanelParent]
    public abstract class BindingViewBase : ViewBase
    {
        protected UIPanelBinder _binder;
        public virtual UIPanelBinder Binder
        {
            get
            {
                if (_binder == null)
                {
                    _binder = new UIPanelBinder(this);
                }
                return _binder;
            }
        }

        private IViewModel _vm;
        public IViewModel VM
        {
            get
            {
                return _vm;
            }
            set
            {
                _vm = value;
                OnViewModelChanged(value);
            }
        }

        protected override void OnBinding(GameObject target)
        {
            base.OnBinding(target);
            var reference = target.GetComponent<BindingReference>();
            if (reference)
            {
                CreateViewModelFromLogicType(reference);
            }
        }

        protected virtual void CreateViewModelFromLogicType(BindingReference reference)
        {
            if (reference.TryLoadLogicScriptType(out var logicType))
            {
                if (typeof(ScriptableObject).IsAssignableFrom(logicType))
                {
                    VM = ScriptableObject.CreateInstance(logicType) as IViewModel;
                }
                else
                {
                    VM = System.Activator.CreateInstance(logicType) as IViewModel;
                }
            }
        }

        public virtual void OnViewModelChanged(IViewModel newValue)
        {
            Binder.Unbind();
            Binder.Bind(newValue);
        }

        protected override void OnClose()
        {
            base.OnClose();
            Binder.Unbind();
        }

        protected override void HandleData(object data)
        {
            base.HandleData(data);

            if (VM != null && VM is IDataReceiver)
            {
                (VM as IDataReceiver).HandleData(data);
            }
        }
    }
}