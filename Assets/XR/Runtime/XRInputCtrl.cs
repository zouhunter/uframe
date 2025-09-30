//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-10-22 10:17:02
//* 描    述： xr手柄输入

//* ************************************************************************************
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace UFrame.XR
{
    public delegate void XRButtonPressEvent(bool left, bool on);
    public delegate void XRAxisMoveEvent(bool left, Vector2 axisValue);
    public delegate void XRGameObjectEvent(bool left, GameObject target);
    public delegate void XRGameObjectPosEvent(bool left, GameObject target, Vector3 pos);

    public class XRInputCtrl :  IUpdate
    {
        protected InputDevice m_leftController;
        protected InputDevice m_rightController;
        protected InputDevice m_headCamera;
        protected bool m_triggerLeftOn;
        protected bool m_triggerRightOn;
        protected bool m_primaryLeftOn;
        protected bool m_primaryRightOn;
        protected bool m_secondLeftOn;
        protected bool m_secondRightOn;
        protected bool m_gripLeftOn;
        protected bool m_gripRightOn;
        protected bool m_axisLeftOn;
        protected bool m_axisRightOn;
        protected Vector2 m_leftAxis;
        protected Vector2 m_rightAxis;
        protected bool m_leftAxisOn;
        protected bool m_rightAxisOn;
        protected Dictionary<GameObject, Vector3> m_leftHoverTargets;
        protected Dictionary<GameObject, Vector3> m_rightHoverTargets;
        protected HashSet<GameObject> m_leftSelectedTargets;
        protected HashSet<GameObject> m_rightSelectedTargets;
        protected Queue<float> m_leftAxisQueue;
        protected Queue<float> m_rightAxisQueue;
        protected bool m_stateValue;
        public float Interval => 0;
        public bool Runing => true;
        public bool LeftAxisOn => m_leftAxisOn;
        public bool RightAxisOn => m_rightAxisOn;
        public Vector2 LeftAxis => m_leftAxis;
        public Vector2 RightAxis => m_rightAxis;
        public float LeftMaxAxis => m_leftAxisQueue.Count > 0 ? Mathf.Max(m_leftAxisQueue.ToArray()) : 0;
        public float RightMaxAxis => m_rightAxisQueue.Count > 0 ? Mathf.Max(m_rightAxisQueue.ToArray()) : 0;
        public bool ExistsLeftSelect => m_leftSelectedTargets?.Count > 0;
        public bool ExistsRightSelect => m_rightSelectedTargets?.Count > 0;
        public bool RightGrabOn => m_gripRightOn;
        public bool LeftGrabOn => m_gripLeftOn;
        public bool LeftTriggerOn => m_triggerLeftOn;
        public bool RightTriggerOn => m_triggerRightOn;
        public InputDevice LeftCtrl => m_leftController;
        public InputDevice RightCtrl => m_rightController;

        public event XRButtonPressEvent onFirstBtnPress;
        public event XRButtonPressEvent onSecondBtnPress;
        public event XRButtonPressEvent onTriggerBtnPress;
        public event XRButtonPressEvent onGrabBtnPress;
        public event XRButtonPressEvent onAxisBtnPress;
        public event XRButtonPressEvent onAxisActive;
        public event XRAxisMoveEvent onAxisMove;
        public event XRGameObjectEvent onSelectTarget;
        public event XRGameObjectEvent onUnSelectTarget;
        public event XRGameObjectPosEvent onHoverTarget;
        public event XRGameObjectEvent onUnHoverTarget;
        public event XRGameObjectPosEvent onTriggerHover;
        public event XRGameObjectEvent onTriggerGrab;

        public XRInputCtrl()
        {
            m_leftSelectedTargets = new HashSet<GameObject>();
            m_rightSelectedTargets = new HashSet<GameObject>();
            m_leftHoverTargets = new Dictionary<GameObject, Vector3>();
            m_rightHoverTargets = new Dictionary<GameObject, Vector3>();
            m_leftAxisQueue = new Queue<float>();
            m_rightAxisQueue = new Queue<float>();
        }

        public virtual void OnRegist()
        {
            var inputDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(inputDevices);
            foreach (var device in inputDevices)
                OnDeviceConnected(device);
            UnityEngine.XR.InputDevices.deviceConnected += OnDeviceConnected;
            UnityEngine.XR.InputDevices.deviceDisconnected += OnDeviceDisConnected;
        }

        public virtual void OnUnRegist()
        {
            UnityEngine.XR.InputDevices.deviceConnected -= OnDeviceConnected;
            UnityEngine.XR.InputDevices.deviceDisconnected -= OnDeviceDisConnected;
        }

        protected virtual void OnDeviceConnected(InputDevice device)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
                {
                    m_leftController = device;
                }
                else if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
                {
                    m_rightController = device;
                }
            }
            else if ((device.characteristics & InputDeviceCharacteristics.HeadMounted) != 0)
            {
                m_headCamera = device;
            }
        }

        protected virtual void OnDeviceDisConnected(InputDevice device)
        {
            Debug.Log("device disconnected:" + device.name);
        }

        public void OnUpdate()
        {
            ProcessInput();
        }

        protected virtual void ProcessInput()
        {
            if (m_leftController.TryGetFeatureValue(CommonUsages.triggerButton, out m_stateValue) && m_stateValue != m_triggerLeftOn)
            {
                m_triggerLeftOn = m_stateValue;
                OnTriggerBtnChanged(true, m_stateValue);
            }

            if (m_rightController.TryGetFeatureValue(CommonUsages.triggerButton, out m_stateValue) && m_stateValue != m_triggerRightOn)
            {
                m_triggerRightOn = m_stateValue;
                OnTriggerBtnChanged(false, m_stateValue);
            }

            if (m_leftController.TryGetFeatureValue(CommonUsages.primaryButton, out m_stateValue) && m_stateValue != m_primaryLeftOn)
            {
                m_primaryLeftOn = m_stateValue;
                OnPrimaryBtnChanged(true, m_stateValue);
            }

            if (m_rightController.TryGetFeatureValue(CommonUsages.primaryButton, out m_stateValue) && m_stateValue != m_primaryRightOn)
            {
                m_primaryRightOn = m_stateValue;
                OnPrimaryBtnChanged(false, m_stateValue);
            }

            if (m_leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out m_stateValue) && m_stateValue != m_secondLeftOn)
            {
                m_secondLeftOn = m_stateValue;
                OnSecondBtnChanged(true, m_stateValue);
            }

            if (m_rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out m_stateValue) && m_stateValue != m_secondRightOn)
            {
                m_secondRightOn = m_stateValue;
                OnSecondBtnChanged(false, m_stateValue);
            }

            if (m_leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out m_leftAxis) && m_leftAxis.magnitude > 0.01f)
            {
                if (!m_leftAxisOn)
                {
                    m_leftAxisOn = true;
                    OnAxisActived(true, m_leftAxisOn);
                }
                if (m_leftAxisQueue.Count > 10)
                {
                    m_leftAxisQueue.Dequeue();
                }
                m_leftAxisQueue.Enqueue(m_leftAxis.magnitude);
                OnAxisChanged(true, m_leftAxis);
            }
            else if (m_leftAxisOn)
            {
                m_leftAxisOn = false;
                OnAxisChanged(true, Vector2.zero);
                OnAxisActived(true, m_leftAxisOn);
            }

            if (m_rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out m_rightAxis) && m_rightAxis.magnitude > 0.01f)
            {
                if (!m_rightAxisOn)
                {
                    m_rightAxisOn = true;
                    OnAxisActived(false, m_rightAxisOn);
                }
                if (m_rightAxisQueue.Count > 10)
                {
                    m_rightAxisQueue.Dequeue();
                }
                m_rightAxisQueue.Enqueue(m_rightAxis.magnitude);
                OnAxisChanged(false, m_rightAxis);
            }
            else if (m_rightAxisOn)
            {
                m_rightAxisOn = false;
                OnAxisChanged(false, Vector2.zero);
                OnAxisActived(false, m_rightAxisOn);
            }

            if (m_leftController.TryGetFeatureValue(CommonUsages.gripButton, out m_stateValue) && m_stateValue != m_gripLeftOn)
            {
                m_gripLeftOn = m_stateValue;
                OnGripBtnChanged(true, m_stateValue);
            }

            if (m_rightController.TryGetFeatureValue(CommonUsages.gripButton, out m_stateValue) && m_stateValue != m_gripRightOn)
            {
                m_gripRightOn = m_stateValue;
                OnGripBtnChanged(false, m_stateValue);
            }
            if (m_leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_stateValue) && m_stateValue != m_axisLeftOn)
            {
                m_axisLeftOn = m_stateValue;
                OnAxisPress(true, m_axisLeftOn);
            }
            if (m_rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out m_stateValue) && m_stateValue != m_axisRightOn)
            {
                m_axisLeftOn = m_stateValue;
                OnAxisPress(false, m_axisLeftOn);
            }
        }

        protected virtual void OnAxisPress(bool left, bool isOn)
        {
            onAxisBtnPress?.Invoke(left, isOn);
        }

        protected virtual void OnGripBtnChanged(bool left, bool isOn)
        {
            onGrabBtnPress?.Invoke(left, isOn);
        }

        protected virtual void OnTriggerBtnChanged(bool left, bool isOn)
        {
            onTriggerBtnPress?.Invoke(left, isOn);
            if (isOn)
            {
                var hoverTargets = left ? m_leftHoverTargets : m_rightHoverTargets;
                var keys = new List<GameObject>(hoverTargets.Keys);
                foreach (var interactorable in keys)
                {
                    if (hoverTargets.TryGetValue(interactorable, out var pos))
                    {
                        onTriggerHover?.Invoke(left, interactorable, pos);
                    }
                }
                var selected = left ? m_leftSelectedTargets : m_rightSelectedTargets;
                foreach (var interactorable in selected)
                {
                    onTriggerGrab?.Invoke(left, interactorable);
                }
            }
        }

        public void ShakeController(bool left, float durection = 0.2f, float amplitude = 0.5f)
        {
            if (left)
                ShakeLeftController(durection, amplitude);
            else
                ShakeRightController(durection, amplitude);
        }

        public void ShakeActiveController()
        {
            if (m_primaryLeftOn || m_triggerLeftOn || m_secondLeftOn || m_gripLeftOn)
            {
                ShakeLeftController();
            }
            if (m_primaryRightOn || m_triggerRightOn || m_secondRightOn || m_gripRightOn)
            {
                ShakeRightController();
            }
        }

        public virtual void ShakeLeftController(float duration = 0.2f, float amplitude = 0.5f)
        {
            if (m_leftController.TryGetHapticCapabilities(out var capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    m_leftController.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }

        public virtual void ShakeRightController(float duration = 0.2f, float amplitude = 0.5f)
        {
            if (m_rightController.TryGetHapticCapabilities(out var capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    m_rightController.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }

        protected virtual void OnPrimaryBtnChanged(bool left, bool isOn)
        {
            onFirstBtnPress?.Invoke(left, isOn);
        }

        protected virtual void OnSecondBtnChanged(bool left, bool isOn)
        {
            onSecondBtnPress?.Invoke(left, isOn);
        }

        protected virtual void OnAxisActived(bool left, bool active)
        {
            onAxisActive?.Invoke(left, active);
        }

        protected virtual void OnAxisChanged(bool left, Vector2 axis)
        {
            onAxisMove?.Invoke(left, axis);
        }

        public void RegistSelected(bool left, GameObject target)
        {
            if (target)
            {
                var selectTargets = left ? m_leftSelectedTargets : m_rightSelectedTargets;
                if (!selectTargets.Contains(target))
                {
                    selectTargets.Add(target);
                    onSelectTarget?.Invoke(left, target);
                }
            }
        }

        public void RemoveSelect(bool left, GameObject target)
        {
            if (target)
            {
                var selectTargets = left ? m_leftSelectedTargets : m_rightSelectedTargets;
                if (selectTargets.Contains(target))
                {
                    selectTargets.Remove(target);
                    onUnSelectTarget?.Invoke(left, target);
                }
            }
        }

        public void SetHoverTarget(bool left, GameObject target, Vector3 hitPos = default)
        {
            if (target)
            {
                var m_hoverTargets = left ? m_leftHoverTargets : m_rightHoverTargets;
                m_hoverTargets[target] = hitPos;
                onHoverTarget?.Invoke(left, target, hitPos);
            }
        }

        public void RemoveHoverTarget(bool left, GameObject target)
        {
            if (target)
            {
                var m_hoverTargets = left ? m_leftHoverTargets : m_rightHoverTargets;
                m_hoverTargets.Remove(target);
                onUnHoverTarget?.Invoke(left, target);
            }
        }

        public bool IsHover(string targetName, bool chekLeft = true, bool checkRight = true)
        {
            if (chekLeft)
                foreach (var pair in m_leftHoverTargets)
                {
                    if (pair.Key.name == targetName)
                    {
                        return true;
                    }
                }
            if (checkRight)
                foreach (var pair in m_rightHoverTargets)
                {
                    if (pair.Key.name == targetName)
                    {
                        return true;
                    }
                }
            return false;
        }

        public bool IsHoverLayer(bool left, int layer)
        {
            var map = left ? m_leftHoverTargets : m_rightHoverTargets;
            if (map != null)
            {
                foreach (var item in map)
                {
                    if (item.Key && item.Key.gameObject.layer == layer)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsSelected(string targetName)
        {
            return IsSelected(true, targetName) || IsSelected(false, targetName);
        }

        public bool IsSelected(bool left, string targetName)
        {
            var selectMap = left ? m_leftSelectedTargets : m_rightSelectedTargets;
            foreach (var pair in selectMap)
            {
                if (pair && pair.name == targetName)
                {
                    return true;
                }
            }
            return false;
        }

        public Dictionary<GameObject, Vector3> GetHoverTargets(bool left)
        {
            return left ? m_leftHoverTargets : m_rightHoverTargets;
        }

        public HashSet<GameObject> GetSelectTargets(bool left)
        {
            return left ? m_leftSelectedTargets : m_rightSelectedTargets;
        }

        public bool ExistsSelect(bool left)
        {
            return left ? ExistsLeftSelect : ExistsRightSelect;
        }
    }
}