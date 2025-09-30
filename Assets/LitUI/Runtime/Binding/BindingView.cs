/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定模式的View                                                                  *
*//************************************************************************************/

using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

namespace UFrame.LitUI
{
    public class BindingView : UIView
    {
        [SerializeReference]
        public VMBinder binder;
        [SerializeReference]
        public ViewModel viewModel;

        public ViewModel VM
        {
            get
            {
                return viewModel;
            }
            set
            {
                if(viewModel != value)
                {
                    viewModel = value;
                    OnViewModelChanged(value);
                }
            }
        }
        public virtual VMBinder Binder
        {
            get
            {
                if (binder == null)
                    binder = new VMBinder();
                return binder;
            }
        }
        protected List<IVar> bindingVars;

        protected override void Awake()
        {
            base.Awake();
            Binder.SetView(this);
            if (viewModel != null)
            {
                VM = viewModel;
                OnViewModelChanged(viewModel);
            }
        }

        /// <summary>
        /// UI打开事件
        /// </summary>
        /// <param name="arg"></param>
        public override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            VM?.OnOpen(arg);
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);
            VM?.SetActive(active);
        }

        /// <summary>
        /// 数据模型变更
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void OnViewModelChanged(ViewModel newValue)
        {
            UnBindVars();
            Binder.Unbind();
            BindingVars(newValue);
            Binder.Bind(newValue);
        }

        /// <summary>
        /// UI关闭事件
        /// </summary>
        protected override void OnClose()
        {
            VM?.OnClose();
            base.OnClose();
            UnBindVars();
            Binder.Unbind();
        }

        /// <summary>
        /// 解绑
        /// </summary>
        protected void UnBindVars()
        {
            if (viewModel != null && bindingVars != null)
            {
                foreach (var f in bindingVars)
                {
                    f.UnBind(viewModel);
                }
            }
        }
        /// <summary>
        /// 属性和事件绑定
        /// </summary>
        /// <param name="viewModel"></param>
        protected void BindingVars(ViewModel viewModel)
        {
            if (viewModel == null)
                return;
            if (bindingVars == null)
            {
                bindingVars = new List<IVar>();
                var fields0 = viewModel.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < fields0.Length; i++)
                {
                    var field = fields0[i];
                    if (typeof(IVar).IsAssignableFrom(field.FieldType))
                    {
                        bindingVars.Add((IVar)field.GetValue(viewModel));
                    }
                }
            }
            foreach (var f in bindingVars)
            {
                if(f is IEvt)
                    f.Bind(viewModel);
            }
            foreach (var f in bindingVars)
            {
                if (!(f is IEvt))
                    f.Bind(viewModel);
            }
        }
    }
}
