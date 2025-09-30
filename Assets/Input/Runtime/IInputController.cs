/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 输入控制器                                                                      *
*//************************************************************************************/

using System;
using UnityEngine;

namespace UFrame.Inputs
{
    public interface IInputController: IKeyboardInputController, IVirtualInputController, IMouseInputController
    {
        Vector3 mousePosition { get; }
        IVirtualValueSetter VirtualSetter { get; }
        IVirtualValueGetter VirtualGetter { get; }
        void SwitchVirtualInput(VirtualInputType inputType);
        //void SetKeyBordActive(bool active); 可添加不触发一组事件的方法
        //void SetMouseActive(bool active);
    }

    public interface IKeyboardInputController
    {
        #region 键盘
        void RegistKeyDown(KeyCode key, System.Action callBack);
        void RemoveKeyDown(KeyCode keyCode, System.Action callBack);
        void RegistKey(KeyCode keyCode, Action callBack);
        void RemoveKey(KeyCode keyCode, Action callBack);
        void RegistKeyUp(KeyCode keyCode, System.Action callBack);
        void RemoveKeyUp(KeyCode keyCode, Action callBack);
        #endregion 键盘
    }

    public interface IVirtualInputController
    {
        #region 摇杆
        void RegistAxis(string axisName, Action<float> callBack);
        void RemoveAxis(string axisName, Action<float> callBack);
        #endregion 摇杆

        #region 虚拟按键盘
        void RegistButtonDown(string buttonName, Action callBack);
        void RemoveButtonDown(string buttonName, Action callBack);
        void RegistButton(string buttonName, Action callBack);
        void RemoveButton(string buttonName, Action callBack);
        void RegistButtonUp(string buttonName, Action callBack);
        void RemoveButtonUp(string buttonName, Action callBack);
        #endregion 虚拟按键
    }

    public interface IMouseInputController
    {
        #region 鼠标
        void RegistMouseDown(int mouseID, System.Action callBack);
        void RemoveMouseDown(int mouseID, Action callBack);
        void RegistMouse(int mouseID, System.Action callBack);
        void RemoveMouse(int mouseID, Action callBack);
        void RegistMouseUp(int mouseID, System.Action callBack);
        void RemoveMouseUp(int mouseID, Action callBack);
        #endregion
    }
}