/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 输入器接口                                                                      *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;


namespace UFrame.Inputs
{
    public abstract class VirtualInput: IVirtualValueGetter, IVirtualValueSetter
    {
        public Vector3 virtualMousePosition { get; private set; }

        protected Dictionary<string, VirtualAxis> m_VirtualAxes;
        protected Dictionary<string, VirtualButton> m_VirtualButtons;
        protected List<string> m_AlwaysUseVirtual;
        // list of the axis and button names that have been flagged to always use a virtual axis or button
        public VirtualInput()
        {
            m_VirtualAxes = new Dictionary<string, VirtualAxis>();
            m_VirtualButtons = new Dictionary<string, VirtualButton>();
            m_AlwaysUseVirtual = new List<string>();
        }
        public virtual void Dispose()
        {
            m_VirtualAxes.Clear();
            m_VirtualButtons.Clear();
            m_AlwaysUseVirtual.Clear();
        }

        public virtual bool AxisExists(string name)
        {
            return m_VirtualAxes.ContainsKey(name);
        }

        public virtual bool ButtonExists(string name)
        {
            return m_VirtualButtons.ContainsKey(name);
        }
        
        public void UnRegisterVirtualAxis(string name)
        {
            // if we have an axis with that name then remove it from our dictionary of registered axes
            if (m_VirtualAxes.ContainsKey(name))
            {
                m_VirtualAxes.Remove(name);
            }
        }

        public void UnRegisterVirtualButton(string name)
        {
            // if we have a button with this name then remove it from our dictionary of registered buttons
            if (m_VirtualButtons.ContainsKey(name))
            {
                m_VirtualButtons.Remove(name);
            }
        }

        public void SetVirtualMousePositionX(float f)
        {
            virtualMousePosition = new Vector3(f, virtualMousePosition.y, virtualMousePosition.z);
        }

        public void SetVirtualMousePositionY(float f)
        {
            virtualMousePosition = new Vector3(virtualMousePosition.x, f, virtualMousePosition.z);
        }

        public void SetVirtualMousePositionZ(float f)
        {
            virtualMousePosition = new Vector3(virtualMousePosition.x, virtualMousePosition.y, f);
        }

        public void SetVirtualMousePosition(Vector3 position)
        {
            virtualMousePosition = position;
        }

        public void SetButtonDown(string name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                AddButton(name);
            }
            m_VirtualButtons[name].Pressed();
        }

        public void SetButtonUp(string name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                AddButton(name);
            }
            m_VirtualButtons[name].Released();
        }

        public void SetAxisPositive(string name)
        {
            if (!m_VirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            m_VirtualAxes[name].Update(1f);
        }

        public void SetAxisNegative(string name)
        {
            if (!m_VirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            m_VirtualAxes[name].Update(-1f);
        }

        public void SetAxisZero(string name)
        {
            if (!m_VirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            m_VirtualAxes[name].Update(0f);
        }

        public void SetAxis(string name, float value)
        {
            if (!m_VirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            m_VirtualAxes[name].Update(value);
        }

        public abstract Vector3 MousePosition();

        protected void AddButton(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            // check if already have a buttin with that name and log an error if we do
            if (m_VirtualButtons.ContainsKey(name))
            {
                Debug.LogError("There is already a virtual button named " + name + " registered.");
            }
            else
            {
                var button = new VirtualButton(name);

                button.onRemove = UnRegisterVirtualButton;
                // add any new buttons
                m_VirtualButtons.Add(button.name, button);

                // if we dont want to match to the input manager then always use a virtual axis
                if (!button.matchWithInputManager)
                {
                    m_AlwaysUseVirtual.Add(button.name);
                }
            }
        }

        protected void AddAxes(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            // check if we already have an axis with that name and log and error if we do
            if (m_VirtualAxes.ContainsKey(name))
            {
                Debug.LogError("There is already a virtual axis named " + name + " registered.");
            }
            else
            {
                var axis = new VirtualAxis(name);

                axis.onRemove = UnRegisterVirtualAxis;
                // add any new axes
                m_VirtualAxes.Add(axis.name, axis);

                // if we dont want to match with the input manager setting then revert to always using virtual
                if (!axis.matchWithInputManager)
                {
                    m_AlwaysUseVirtual.Add(axis.name);
                }
            }
        }

        protected float GetVirtualAxis(string name, bool raw)
        {
            if (!m_VirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            return m_VirtualAxes[name].GetValue;
        }

        protected bool GetVirtualButtonDown(string name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonDown;
            }

            AddButton(name);
            return m_VirtualButtons[name].GetButtonDown;
        }

        protected bool GetVirtualButtonUp(string name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonUp;
            }

            AddButton(name);
            return m_VirtualButtons[name].GetButtonUp;
        }

        protected bool GetVirtualButton(string name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButton;
            }

            AddButton(name);
            return m_VirtualButtons[name].GetButton;
        }

        public abstract float GetAxis(string name, bool raw);
        public abstract bool GetButton(string name);
        public abstract bool GetButtonDown(string name);
        public abstract bool GetButtonUp(string name);

        public float GetAxis(string name)
        {
            return GetAxis(name, false);
        }

        public float GetAxisRaw(string name)
        {
            return GetAxis(name, true);
        }
    }
}
