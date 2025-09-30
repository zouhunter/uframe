//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2022-07-06 14:36:45
//* 描    述： 键盘鼠标输入管理器

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Inputs
{
    public class VertialInputCtrl
    {
        public bool Runing => true;
        public float Interval => 0;
        public KeyCode triggerKey = KeyCode.LeftShift;
        public HashSet<KeyCode> sequenceKeyCodes = new HashSet<KeyCode>();
        public event Action<Vector2> dragEvent;
        public event Action<float> scrollEvent;
        public bool gmActive = true;

        protected Dictionary<string, List<Action>> m_processs = new Dictionary<string, List<Action>>();
        protected Dictionary<string, List<Action<string>>> m_genericProcesss = new Dictionary<string, List<Action<string>>>();
        protected string m_currentProcess;
        protected Dictionary<KeyCode, List<Action>> m_keyDownActions = new Dictionary<KeyCode, List<Action>>();
        protected Dictionary<KeyCode, List<Action>> m_keyUpActions = new Dictionary<KeyCode, List<Action>>();
        protected Dictionary<KeyCode, List<Action>> m_keyActions = new Dictionary<KeyCode, List<Action>>();
        protected HashSet<KeyCode> m_pressedKeys = new HashSet<KeyCode>();
        protected Dictionary<int, List<Action>> m_mouseDownActions = new Dictionary<int, List<Action>>();
        protected Dictionary<int, List<Action>> m_mouseUpActions = new Dictionary<int, List<Action>>();
        protected Dictionary<int, List<Action>> m_mouseActions = new Dictionary<int, List<Action>>();
        protected HashSet<int> m_mousePressedKeys = new HashSet<int>();
        protected float m_touchDistance;
        protected bool m_touchDown;
        protected Vector2 m_dragPos;
        protected bool m_scaling;
        protected List<int> m_runingMouseKeys = new List<int>();
        protected List<KeyCode> m_runingKeys = new List<KeyCode>();
        protected List<Action> m_runingActions = new List<Action>();

        public void OnUpdate()
        {
            if (!Application.isMobilePlatform)
            {
                if(gmActive)
                {
                    ProcessGmInput();
                }
                ProcessKeyInput();
            }
            ProcessTouchInput();
            TriggerPressedActions();
            ProcessDrag();
            ProcessScroll();
        }

        public void OnLateUpdate()
        {
            var notTouch = Application.isMobilePlatform && Input.touchCount <= 0;
            var notKey = !Application.isMobilePlatform && !Input.anyKey;
            if ((notTouch || notKey) && m_mousePressedKeys.Count > 0)
            {
                m_runingMouseKeys.Clear();
                m_runingMouseKeys.AddRange(m_mousePressedKeys);
                foreach (var code in m_runingMouseKeys)
                {
                    m_mousePressedKeys.Remove(code);
                    SetMouseUp(code);
                }
            }
        }

        protected void ProcessScroll()
        {
            if (scrollEvent != null)
            {
                if (!Application.isMobilePlatform)
                {
                    var scoll = Input.GetAxis("Mouse ScrollWheel");
                    if (scoll != 0)
                    {
                        m_scaling = true;
                        scrollEvent?.Invoke(scoll);
                    }
                    else
                    {
                        m_scaling = false;
                    }
                }
                else if (Input.touchCount >= 2)
                {
                    Touch newTouch1 = Input.GetTouch(0);
                    Touch newTouch2 = Input.GetTouch(1);
                    var distance = Vector2.Distance(newTouch1.position, newTouch2.position);
                    if (m_touchDistance > 1 && scrollEvent != null)
                    {
                        scrollEvent.Invoke(distance - m_touchDistance);
                    }
                    m_touchDistance = distance;
                    m_scaling = true;
                }
            }
        }

        protected void ProcessDrag()
        {
            if (dragEvent != null)
            {
                if (!Application.isMobilePlatform && Input.GetMouseButton(0) && !m_scaling)
                {
                    if (!m_touchDown)
                    {
                        m_dragPos = Input.mousePosition;
                        m_touchDown = true;
                    }
                    else
                    {
                        var pos = Input.mousePosition;
                        var offset = new Vector2(pos.x, pos.y) - m_dragPos;
                        m_dragPos = pos;
                        dragEvent?.Invoke(offset);
                    }
                }
                else if (Input.touchCount == 1)
                {
                    if (!m_touchDown)
                    {
                        m_dragPos = Input.GetTouch(0).position;
                        m_touchDown = true;
                    }
                    else
                    {
                        var pos = Input.GetTouch(0).position;
                        var offset = pos - m_dragPos;
                        m_dragPos = pos;
                        dragEvent?.Invoke(offset);
                    }
                    m_scaling = false;
                    m_touchDistance = 0;
                }

            }

            if (m_touchDown)
            {
                m_touchDown = false;
                m_scaling = false;
                m_touchDistance = 0;
            }
        }

        protected void TriggerPressedActions()
        {
            if (m_pressedKeys.Count > 0)
            {
                m_runingKeys.Clear();
                m_runingKeys.AddRange(m_pressedKeys);
                foreach (var key in m_runingKeys)
                {
                    if (m_keyActions.TryGetValue(key, out var actions))
                    {
                        InvokeActions(actions);
                    }
                }
            }
            if (m_mousePressedKeys.Count > 0)
            {
                m_runingMouseKeys.Clear();
                m_runingMouseKeys.AddRange(m_mousePressedKeys);
                foreach (var key in m_runingMouseKeys)
                {
                    if (m_mouseActions.TryGetValue(key, out var actions))
                    {
                        InvokeActions(actions);
                    }
                }
            }
        }

        protected void ProcessTouchInput()
        {
            if (m_mouseDownActions.Count > 0)
            {
                m_runingMouseKeys.Clear();
                m_runingMouseKeys.AddRange(m_mouseDownActions.Keys);
                foreach (var key in m_runingMouseKeys)
                {
                    if (Input.GetMouseButtonDown(key))
                        SetMouseDown(key);
                }
            }
            if (m_mouseUpActions.Count > 0)
            {
                m_runingMouseKeys.Clear();
                m_runingMouseKeys.AddRange(m_mouseUpActions.Keys);
                foreach (var key in m_runingMouseKeys)
                {
                    if (Input.GetMouseButtonUp(key))
                        SetMouseUp(key);
                }
            }
        }

        protected void ProcessKeyInput()
        {
            if (m_keyDownActions.Count > 0)
            {
                m_runingKeys.Clear();
                m_runingKeys.AddRange(m_keyDownActions.Keys);
                foreach (var key in m_runingKeys)
                {
                    if (Input.GetKeyDown(key))
                        SetKeyDown(key);
                }
            }

            if (m_keyUpActions.Count > 0)
            {
                m_runingKeys.Clear();
                m_runingKeys.AddRange(m_keyUpActions.Keys);
                foreach (var key in m_runingKeys)
                {
                    if (Input.GetKeyUp(key))
                        SetKeyUp(key);
                }
            }
        }

        protected void ProcessGmInput()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                m_currentProcess = "";
            }
            else if (Input.GetKeyUp(triggerKey))
            {
                DoCommand(m_currentProcess);
            }
            else if (Input.GetKey(triggerKey))
            {
                foreach (var key in sequenceKeyCodes)
                {
                    if (Input.GetKeyDown(key))
                    {
                        m_currentProcess += (char)key;
                    }
                }
            }
        }

        public void RegistCmd(string keyword, Action callback)
        {
            if (m_processs.TryGetValue(keyword, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_processs[keyword] = new List<Action>() { callback };
            }
        }

        public void RemoveCmd(string keyword, Action callback)
        {
            if (m_processs.TryGetValue(keyword, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }

        public void RegistCmd(string keyword, Action<string> callback)
        {
            if (m_genericProcesss.TryGetValue(keyword, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_genericProcesss[keyword] = new List<Action<string>>() { callback };
            }
        }

        public void RemoveCmd(string keyword, Action<string> callback)
        {
            if (m_genericProcesss.TryGetValue(keyword, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }

        public void DoCommand(string keyward)
        {
            if(!gmActive)
            {
                Debug.Log("gmCommand forbid!");
                return;
            }

            if (m_processs.TryGetValue(keyward, out var actions))
            {
                InvokeActions(actions);
                Debug.Log("finish execute command:" + keyward);
            }
            foreach (var pair in m_genericProcesss)
            {
                if(keyward.StartsWith(keyward))
                {
                    InvokeActions(pair.Value, keyward);
                }
            }
        }


        protected void InvokeActions(List<Action> actions)
        {
            if (actions == null || actions.Count <= 0)
                return;
            m_runingActions.Clear();
            m_runingActions.AddRange(actions);
            foreach (var item in m_runingActions)
            {
                try
                {
                    item?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("action exception:" + e.Message);
                }
            }
        }

        protected void InvokeActions(List<Action<string>> actions,string args)
        {
            if (actions == null || actions.Count <= 0)
                return;
            var runingActions = actions.ToArray();
            foreach (var item in runingActions)
            {
                try
                {
                    item?.Invoke(args);
                }
                catch (Exception e)
                {
                    Debug.LogError("action exception:" + e.Message);
                }
            }
        }

        public void RegistMouseDown(int code, System.Action callback)
        {
            if (m_mouseDownActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_mouseDownActions[code] = new List<Action>() { callback };
            }
        }
        public void RegistMouse(int code, System.Action callback)
        {
            if (m_mouseActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_mouseActions[code] = new List<Action>() { callback };
            }
        }
        public void RegistMouseUp(int code, System.Action callback)
        {
            if (m_mouseUpActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_mouseUpActions[code] = new List<Action>() { callback };
            }
        }


        public void RemoveMouseDown(int code, System.Action callback)
        {
            if (m_mouseDownActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }
        public void RemoveMouse(int code, System.Action callback)
        {
            if (m_mouseActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }
        public void RemoveMouseUp(int code, System.Action callback)
        {
            if (m_mouseUpActions.TryGetValue(code, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }

        public void SetMouseDown(int code)
        {
            m_mousePressedKeys.Add(code);
            if (m_mouseDownActions.TryGetValue(code, out var actions))
            {
                InvokeActions(actions);
            }
        }

        public void SetMouseUp(int code)
        {
            m_mousePressedKeys.Remove(code);
            if (m_mouseUpActions.TryGetValue(code, out var actions))
            {
                InvokeActions(actions);
            }
        }

        public void RegistKeyDown(KeyCode key, System.Action callback)
        {
            if (m_keyDownActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_keyDownActions[key] = new List<Action>() { callback };
            }
        }

        public void RegistKeyUp(KeyCode key, System.Action callback)
        {
            if (m_keyUpActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_keyUpActions[key] = new List<Action>() { callback };
            }
        }
        public void RegistKeyEvent(KeyCode key, System.Action callback)
        {
            if (m_keyActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Add(callback);
            }
            else
            {
                m_keyActions[key] = new List<Action>() { callback };
            }
        }

        public void RemoveKeyDown(KeyCode key, System.Action callback)
        {
            if (m_keyDownActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }

        public void RemoveKeyUp(KeyCode key, System.Action callback)
        {
            if (m_keyUpActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }
        public void RemoveKeyEvent(KeyCode key, System.Action callback)
        {
            if (m_keyActions.TryGetValue(key, out var actions) && actions != null)
            {
                actions.Remove(callback);
            }
        }

        public void SetKeyDown(KeyCode key)
        {
            m_pressedKeys.Add(key);
            if (m_keyDownActions.TryGetValue(key, out var actions))
            {
                InvokeActions(actions);
            }
        }

        public void SetKeyUp(KeyCode key)
        {
            m_pressedKeys.Remove(key);
            if (m_keyUpActions.TryGetValue(key, out var actions))
            {
                InvokeActions(actions);
            }
        }

        public bool IsKeyPressed(KeyCode key)
        {
            return m_pressedKeys.Contains(key);
        }
    }
}