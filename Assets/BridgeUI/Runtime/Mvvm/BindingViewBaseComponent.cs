/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定模式的View                                                                  *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public abstract class BindingViewBaseComponent : ViewBaseComponent
    {
        protected UIPanelBinder _binder;
        public virtual UIPanelBinder Binder
        {
            get
            {
                if (_binder == null)
                {
                    _binder = new UIPanelBinder(view);
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

        public virtual void OnViewModelChanged(IViewModel newValue)
        {
            Binder.Unbind();
            Binder.Bind(newValue);
        }
        protected override void OnMessageReceive(object message)
        {
            base.OnMessageReceive(message);
            if (VM != null && VM is IDataReceiver)
            {
                (VM as IDataReceiver).HandleData(message);
            }
        }
        protected override void OnRecover()
        {
            base.OnRecover();
            Binder.Unbind();
        }
    }
}