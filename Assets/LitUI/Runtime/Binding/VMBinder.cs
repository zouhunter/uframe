/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定器                                                                          *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using System;
namespace UFrame.LitUI
{
    public class VMBinder
    {
        public BindingView Context { get; private set; }
        public IViewModel viewModel { get; private set; }
        private event UnityAction<IViewModel> binders;
        private event UnityAction<IViewModel> unbinders;
        public virtual void SetView(BindingView panel)
        {
            this.Context = panel;
        }
        public T GetRef<T>(string name) where T:UnityEngine.Object
        {
            return Context?.GetRef<T>(name);
        }
        #region UnityEvent
        /// <summary>
        /// 注册通用事件
        /// </summary>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent(UnityEvent uEvent, string sourceName)
        {
            UnityAction action = () =>
            {
                InvokeEvent(sourceName);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }


        /// <summary>
        /// 注册通用事件
        /// </summary>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T>(UnityEvent uEvent, string sourceName, T target)
        {
            UnityAction action = () =>
            {
                InvokeEvent(sourceName, target);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        #endregion

        #region UnityEvent<T>

        /// <summary>
        /// 注册状态改变事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T>(UnityEvent<T> uEvent, string sourceName)
        {
            UnityAction<T> action = (x) =>
            {
                InvokeEvent<T>(sourceName, x);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }


        /// <summary>
        /// 注册事件并传递指定参数
        /// (其中arguments中的参数只能是引用类型,否则无法正常显示)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, P>(UnityEvent<T> uEvent, string sourceName, P target)
        {
            UnityAction<T> action = (arg0) =>
            {
                InvokeEvent(sourceName, arg0, target);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }

        #endregion

        #region UnityEvent<T, S>
        /// <summary>
        /// 注册状态改变事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, S>(UnityEvent<T, S> uEvent, string sourceName)
        {
            UnityAction<T, S> action = (arg0, arg1) =>
            {
                InvokeEvent(sourceName, arg0, arg1);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        /// <summary>
        /// 注册事件并传递指定参数
        /// (其中arguments中的参数只能是引用类型,否则无法正常显示)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, X, P>(UnityEvent<T, X> uEvent, string sourceName, P target)
        {
            UnityAction<T, X> action = (arg0, arg1) =>
            {
                InvokeEvent(sourceName, arg0, arg1, target);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }

        #endregion

        #region UnityEvent<T, X, Y>

        /// <summary>
        /// 注册状态改变事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, S, R>(UnityEvent<T, S, R> uEvent, string sourceName)
        {
            UnityAction<T, S, R> action = (x, y, z) =>
            {
                InvokeEvent(sourceName, x, y, z);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        /// <summary>
        /// 注册事件并传递指定参数
        /// (其中arguments中的参数只能是引用类型,否则无法正常显示)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, X, Y, P>(UnityEvent<T, X, Y> uEvent, string sourceName, P target)
        {
            UnityAction<T, X, Y> action = (x, y, z) =>
            {
                InvokeEvent(sourceName, x, y, z, target);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }

        #endregion

        #region UnityEvent<T, X, Y, Z>

        /// <summary>
        /// 注册状态改变事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, S, R, Q>(UnityEvent<T, S, R, Q> uEvent, string sourceName)
        {
            UnityAction<T, S, R, Q> action = (x, y, z, w) =>
            {
                InvokeEvent(sourceName, x, y, z, w);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        /// <summary>
        /// 注册事件并传递指定参数
        /// (其中arguments中的参数只能是引用类型,否则无法正常显示)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, X, Y, Z, P>(UnityEvent<T, X, Y, Z> uEvent, string sourceName, P target)
        {
            UnityAction<T, X, Y, Z> action = (x, y, z, w) =>
            {
                InvokeEvent(sourceName, x, y, z, w, target);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        #endregion

        #region  触发事件
        #region Bind&UnBind
        public virtual void Bind(IViewModel viewModel)
        {
            Debug.Log("Bind:" + viewModel);
            this.viewModel = viewModel;

            if (viewModel != null)
            {
                if (binders != null)
                {
                    binders.Invoke(viewModel);
                }

                viewModel.OnAfterBinding(Context);
            }
        }
        public virtual void Unbind()
        {
            Debug.Log("UnBind:" + viewModel);
            if (viewModel != null)
            {
                viewModel.OnBeforeUnBinding(Context);

                if (unbinders != null)
                {
                    unbinders.Invoke(viewModel);
                }
            }
            this.viewModel = null;
        }
        #endregion

        #region InvokeEvents
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent(string sourceName)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                viewModel.SetActiveView(Context);
                func.Invoke();
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent<T>(string sourceName, T arg0)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action<T>>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                func.Invoke(arg0);
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent<S, T>(string sourceName, S arg0, T arg1)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action<S, T>>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                func.Invoke(arg0, arg1);
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent<R, S, T>(string sourceName, R arg0, S arg1, T arg2)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action<R, S, T>>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                func.Invoke(arg0, arg1, arg2);
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent<Q, R, S, T>(string sourceName, Q arg0, R arg1, S arg2, T arg3)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action<Q, R, S, T>>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                func.Invoke(arg0, arg1, arg2, arg3);
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        /// <summary>
        /// 触发方法
        /// </summary>
        /// <param name="sourceName"></param>
        public virtual void InvokeEvent<P, Q, R, S, T>(string sourceName, P arg0, Q arg1, R arg2, S arg3, T arg4)
        {
            if (viewModel == null) return;

            var prop = viewModel.GetBindableProperty<Action<P, Q, R, S, T>>(sourceName);
            if (prop != null && prop.Value != null)
            {
                var func = prop.Value;
                func.Invoke(arg0, arg1, arg2, arg3, arg4);
            }
            else
            {
                Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
            }
        }
        #endregion
        #endregion

        #region RegistValueEvent
        /// <summary>
        /// 注册状态改变事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistValueEvent<T>(UnityEvent<T> uEvent, string sourceName)
        {
            UnityAction<T> action = (x) =>
            {
                SetValue<T>(x, sourceName);
            };

            if (viewModel != null)
            {
                uEvent.AddListener(action);
            }
            else
            {
                binders += viewModel =>
                {
                    uEvent.AddListener(action);
                };
            }

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        #endregion

        #region RegistValueChange 
        /// <summary>
        /// 手动指定绑定事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceName"></param>
        /// <param name="onViewModelChanged"></param>
        public virtual void RegistValueChange<T>(UnityAction<T> onViewModelChanged, string sourceName,UnityEvent<T> changeEvent = null)
        {
            if (onViewModelChanged == null) return;

            if (viewModel != null)
            {
                var prop = viewModel.GetBindableProperty<T>(sourceName);

                var haveDefult = viewModel.HaveDefultProperty(sourceName);

                if (haveDefult && prop.Value != null)
                {
                    onViewModelChanged.Invoke(prop.Value);
                }

                //Debug.Log(sourceName + " porp:" + prop.Value + ":" + prop.GetHashCode());
                prop.RegistValueChanged(onViewModelChanged);
                changeEvent?.AddListener(prop.SetValueNoTrigger);
            }
            else
            {
                binders += (viewModel) =>
                {
                    var haveDefult = viewModel.HaveDefultProperty(sourceName);

                    var prop = viewModel.GetBindableProperty<T>(sourceName);

                    if (haveDefult && prop.Value != null)
                    {
                        onViewModelChanged.Invoke(prop.Value);
                    }
                    //Debug.Log(sourceName + " porp:" + prop.Value + ":" + prop.GetHashCode());
                    prop.RegistValueChanged(onViewModelChanged);
                    changeEvent?.AddListener(prop.SetValueNoTrigger);
                };
            }

            unbinders += (viewModel) =>
            {
                var prop = viewModel.GetBindableProperty<T>(sourceName);
                prop.RemoveValueChanged(onViewModelChanged);
                changeEvent?.RemoveListener(prop.SetValueNoTrigger);
            };
        }
        #endregion

        #region SetValue
        /// <summary>
        /// 设置viewModel中变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="sourceName"></param>
        public virtual void SetValue<T>(T value, string sourceName)
        {
            if (viewModel != null)
            {
                var prop = viewModel.GetBindableProperty<T>(sourceName);
                prop.Value = value;
            }
            else
            {
                binders += (viewModel) =>
                {
                    var prop = viewModel.GetBindableProperty<T>(sourceName);
                    prop.Value = value;
                };
            }
        }
        /// <summary>
        /// 设置viewModel中变量的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="sourceName"></param>
        public virtual void SetBoxValue(object value, string sourceName)
        {
            if (viewModel != null)
            {
                var prop = viewModel.GetBindableProperty(sourceName, value.GetType());
                if (prop != null)
                {
                    prop.ValueBoxed = value;
                }
            }
            else
            {
                binders += (viewModel) =>
                {
                    var prop = viewModel.GetBindableProperty(sourceName, value.GetType());
                    if (prop != null)
                    {
                        prop.ValueBoxed = value;
                    }
                };
            }
        }
        #endregion
    }
}
