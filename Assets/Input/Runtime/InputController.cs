/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 输入控制器                                                                      *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UFrame;

namespace UFrame.Inputs
{
    public class InputController : IInputController,IUpdate
    {
        private CrossPlatformInputCtrl crossPlantformCtrl;
        private List<IInputHandle> inputJudges;
        private bool stateChanged;

        private KeyDownInputJudge keyDownInputJudge;
        private KeyInputJudge keyInputJudge;
        private KeyUpInputJudge keyUpInputJudge;

        private MouseDownInputJudge mouseDownInputJudge;
        private MouseInputJudge mouseInputJudge;
        private MouseUpInutJudge mouseUpInputJudge;

        private ButtonDownInputJudge buttonDownInputJudge;
        private ButtonInputJudge buttonInputJudge;
        private ButtonUpInputJudge buttonUpInputJudge;

        private AxisInputHandle axisInputHandle;

        public Vector3 mousePosition
        {
            get
            {
                if (CheckAlive())
                {
                    return crossPlantformCtrl.mousePosition;
                }
                return Vector3.zero;
            }
        }
        public IVirtualValueSetter VirtualSetter
        {
            get
            {
                if (CheckAlive())
                {
                    return crossPlantformCtrl;
                }
                return null;
            }
        }
        public IVirtualValueGetter VirtualGetter
        {
            get
            {
                if (CheckAlive())
                {
                    return crossPlantformCtrl;
                }
                return null;
            }
        }

        public bool Regsited { get; protected set; }

        public float Interval => 0;

        public bool Runing => true;

        private Stack<IInputHandle> waitAddHandles;
        private Stack<IInputHandle> waitRemoveHandles;

        protected bool CheckAlive()
        {
            return Regsited;
        }

        public void OnRegist()
        {
            crossPlantformCtrl = new CrossPlatformInputCtrl();

            mouseDownInputJudge = new MouseDownInputJudge();
            mouseInputJudge = new MouseInputJudge();
            mouseUpInputJudge = new MouseUpInutJudge();

            keyDownInputJudge = new KeyDownInputJudge();
            keyInputJudge = new KeyInputJudge();
            keyUpInputJudge = new KeyUpInputJudge();

            buttonDownInputJudge = new ButtonDownInputJudge(crossPlantformCtrl);
            buttonInputJudge = new ButtonInputJudge(crossPlantformCtrl);
            buttonUpInputJudge = new ButtonUpInputJudge(crossPlantformCtrl);

            axisInputHandle = new AxisInputHandle(crossPlantformCtrl);

            inputJudges = new List<IInputHandle>()
            {
                mouseDownInputJudge,mouseInputJudge,mouseUpInputJudge,
                keyDownInputJudge,keyInputJudge,keyUpInputJudge,
                buttonDownInputJudge,buttonInputJudge,buttonUpInputJudge,
                axisInputHandle
            };

            using (var enumerator = inputJudges.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var judge = enumerator.Current;
                    judge.onError = Debug.LogError;
                    judge.onEmpty = OnRemoveFromLiterator;
                    judge.onNotEmpty = OnAddToLiterator;
                }
            }

            inputJudges.Clear();
            waitAddHandles = new Stack<IInputHandle>();
            waitRemoveHandles = new Stack<IInputHandle>();
            Regsited = true;
        }

        private void OnAddToLiterator(IInputHandle obj)
        {
            if (!inputJudges.Contains(obj))
            {
                stateChanged = true;
                waitAddHandles.Push(obj);
            }
        }

        private void OnRemoveFromLiterator(IInputHandle obj)
        {
            if (inputJudges.Contains(obj))
            {
                stateChanged = true;
                waitRemoveHandles.Push(obj);
            }
        }

        public void OnUnRegist()
        {
            mouseDownInputJudge.Dispose();
            mouseInputJudge.Dispose();
            mouseUpInputJudge.Dispose();

            keyDownInputJudge.Dispose();
            keyInputJudge.Dispose();
            keyUpInputJudge.Dispose();

            buttonDownInputJudge.Dispose();
            buttonInputJudge.Dispose();
            buttonUpInputJudge.Dispose();

            axisInputHandle.Dispose();
            crossPlantformCtrl.Dispose();

            inputJudges.Clear();
            waitRemoveHandles.Clear();
            waitAddHandles.Clear();
            Regsited = false;
        }

        public void SwitchVirtualInput(VirtualInputType inputType)
        {
            if (!CheckAlive()) return;
            crossPlantformCtrl.SwitchActiveInputMethod(inputType);
        }

        public void OnUpdate()
        {
            if (inputJudges.Count > 0)
            {
                using (var enumerator = inputJudges.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var judge = enumerator.Current;
                        if (judge.HaveEvent())
                        {
                            judge.Execute();
                        }
                    }
                }
            }

            if(stateChanged)
            {
                stateChanged = false;
                while (waitAddHandles.Count > 0)
                {
                    var item = waitAddHandles.Pop();
                    inputJudges.Add(item);
                }

                while (waitRemoveHandles.Count > 0)
                {
                    var item = waitRemoveHandles.Pop();
                    inputJudges.Remove(item);
                }
            }
        }

        #region 键盘
        public void RegistKey(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyInputJudge.RegistEvent((int)keyCode, callBack);
        }

        public void RemoveKey(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyInputJudge.RemoveEvent((int)keyCode, callBack);
        }

        public void RegistKeyDown(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyDownInputJudge.RegistEvent((int)keyCode, callBack);
        }

        public void RemoveKeyDown(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyDownInputJudge.RemoveEvent((int)keyCode, callBack);
        }

        public void RegistKeyUp(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyUpInputJudge.RegistEvent((int)keyCode, callBack);
        }
        public void RemoveKeyUp(KeyCode keyCode, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            keyUpInputJudge.RemoveEvent((int)keyCode, callBack);
        }
        #endregion 键盘

        #region 鼠标
        public void RegistMouseDown(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseDownInputJudge.RegistEvent(mouseID, callBack);
        }

        public void RemoveMouseDown(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseDownInputJudge.RemoveEvent(mouseID, callBack);
        }

        public void RegistMouse(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseInputJudge.RegistEvent(mouseID, callBack);
        }

        public void RemoveMouse(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseInputJudge.RemoveEvent(mouseID, callBack);
        }

        public void RegistMouseUp(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseUpInputJudge.RegistEvent(mouseID, callBack);
        }

        public void RemoveMouseUp(int mouseID, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            mouseUpInputJudge.RemoveEvent(mouseID, callBack);
        }
        #endregion 鼠标

        #region 轴
        public void RegistAxis(string axisName, Action<float> callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            axisInputHandle.RegistEvent(axisName, callBack);
        }

        public void RemoveAxis(string axisName, Action<float> callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            axisInputHandle.RemoveEvent(axisName, callBack);
        }
        #endregion

        #region 虚拟按键

        public void RegistButtonDown(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonDownInputJudge.RegistEvent(buttonName, callBack);
        }

        public void RemoveButtonDown(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonDownInputJudge.RemoveEvent(buttonName, callBack);
        }
        public void RegistButton(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonInputJudge.RegistEvent(buttonName, callBack);
        }

        public void RemoveButton(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonInputJudge.RemoveEvent(buttonName, callBack);
        }

        public void RegistButtonUp(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonUpInputJudge.RegistEvent(buttonName, callBack);
        }

        public void RemoveButtonUp(string buttonName, Action callBack)
        {
            if (!CheckAlive()) return;
            if (callBack == null) return;
            buttonUpInputJudge.RemoveEvent(buttonName, callBack);
        }
        #endregion 虚拟按键
    }

}