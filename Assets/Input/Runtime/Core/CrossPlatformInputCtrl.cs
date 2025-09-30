/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 跨平台输入管理器                                                                *
*//************************************************************************************/

using System;
using UnityEngine;
using UFrame.Inputs.PlatformSpecific;

namespace UFrame.Inputs
{
    // virtual axis and button classes - applies to mobile input
    // Can be mapped to touch joysticks, tilt, gyro, etc, depending on desired implementation.
    // Could also be implemented by other input devices - kinect, electronic sensors, etc
    public class VirtualAxis
    {
        public string name { get; private set; }
        private float m_Value;
        public bool matchWithInputManager { get; private set; }
        public System.Action<string> onRemove { get; set; }


        public VirtualAxis(string name)
            : this(name, true)
        {
        }


        public VirtualAxis(string name, bool matchToInputSettings)
        {
            this.name = name;
            matchWithInputManager = matchToInputSettings;
        }


        // removes an axes from the cross platform input system
        public void Remove()
        {
            onRemove(name);
        }


        // a controller gameobject (eg. a virtual thumbstick) should update this class
        public void Update(float value)
        {
            m_Value = value;
        }


        public float GetValue
        {
            get { return m_Value; }
        }


        public float GetValueRaw
        {
            get { return m_Value; }
        }
    }

    // a controller gameobject (eg. a virtual GUI button) should call the
    // 'pressed' function of this class. Other objects can then read the
    // Get/Down/Up state of this button.
    public class VirtualButton
    {
        public string name { get; private set; }
        public bool matchWithInputManager { get; private set; }

        private int m_LastPressedFrame = -5;
        private int m_ReleasedFrame = -5;
        private bool m_Pressed;
        public System.Action<string> onRemove { get; set; }

        public VirtualButton(string name)
            : this(name, true)
        {
        }


        public VirtualButton(string name, bool matchToInputSettings)
        {
            this.name = name;
            matchWithInputManager = matchToInputSettings;
        }


        // A controller gameobject should call this function when the button is pressed down
        public void Pressed()
        {
            if (m_Pressed)
            {
                return;
            }
            m_Pressed = true;
            m_LastPressedFrame = Time.frameCount;
        }


        // A controller gameobject should call this function when the button is released
        public void Released()
        {
            m_Pressed = false;
            m_ReleasedFrame = Time.frameCount;
        }


        // the controller gameobject should call Remove when the button is destroyed or disabled
        public void Remove()
        {
            onRemove(name);
        }


        // these are the states of the button which can be read via the cross platform input system
        public bool GetButton
        {
            get { return m_Pressed; }
        }


        public bool GetButtonDown
        {
            get
            {
                return m_LastPressedFrame - Time.frameCount == -1;
            }
        }


        public bool GetButtonUp
        {
            get
            {
                return (m_ReleasedFrame == Time.frameCount - 1);
            }
        }
    }

    public enum VirtualInputType
    {
        Hardware,
        Touch
    }

    public class CrossPlatformInputCtrl:IVirtualValueGetter,IVirtualValueSetter
    {
        private MobileInput s_TouchInput;
        private StandaloneInput s_HardwareInput;
        private VirtualInput activeInput;

        public CrossPlatformInputCtrl()
        {
            s_TouchInput = new MobileInput();
            s_HardwareInput = new StandaloneInput();

            if(Application.isMobilePlatform)
            {
                activeInput = s_TouchInput;
            }
            else
            {
                activeInput = s_HardwareInput;
            }
        }

        public void Dispose()
        {
            s_TouchInput.Dispose();
            s_HardwareInput.Dispose();
        }

        public void SwitchActiveInputMethod(VirtualInputType activeInputMethod)
        {
            switch (activeInputMethod)
            {
                case VirtualInputType.Hardware:
                    activeInput = s_HardwareInput;
                    break;

                case VirtualInputType.Touch:
                    activeInput = s_TouchInput;
                    break;
            }
        }

        public bool AxisExists(string name)
        {
            return activeInput.AxisExists(name);
        }

        public bool ButtonExists(string name)
        {
            return activeInput.ButtonExists(name);
        }

        public float GetAxis(string name)
        {
            return activeInput.GetAxis(name);
        }

        public float GetAxisRaw(string name)
        {
            return activeInput.GetAxisRaw(name);
        }

        public bool GetButton(string name)
        {
            return activeInput.GetButton(name);
        }

        public bool GetButtonDown(string name)
        {
            return activeInput.GetButtonDown(name);
        }

        public bool GetButtonUp(string name)
        {
            return activeInput.GetButtonUp(name);
        }

        public void SetButtonDown(string name)
        {
            activeInput.SetButtonDown(name);
        }

        public void SetButtonUp(string name)
        {
            activeInput.SetButtonUp(name);
        }

        public void SetAxisPositive(string name)
        {
            activeInput.SetAxisPositive(name);
        }

        public void SetAxisNegative(string name)
        {
            activeInput.SetAxisNegative(name);
        }

        public void SetAxisZero(string name)
        {
            activeInput.SetAxisZero(name);
        }

        public void SetAxis(string name, float value)
        {
            activeInput.SetAxis(name,value);
        }

        public void UnRegisterVirtualAxis(string name)
        {
            activeInput.UnRegisterVirtualAxis(name);
        }

        public void SetVirtualMousePosition(Vector3 position)
        {
            activeInput.SetVirtualMousePosition(position);
        }

        public void SetVirtualMousePositionX(float f)
        {
            activeInput.SetVirtualMousePositionX(f);
        }

        public void SetVirtualMousePositionY(float f)
        {
            activeInput.SetVirtualMousePositionY(f);
        }

        public void SetVirtualMousePositionZ(float f)
        {
            activeInput.SetVirtualMousePositionZ(f);
        }

        public void UnRegisterVirtualButton(string name)
        {
            activeInput.UnRegisterVirtualButton(name);
        }

        public Vector3 mousePosition
        {
            get { return activeInput.MousePosition(); }
        }

    }
}
