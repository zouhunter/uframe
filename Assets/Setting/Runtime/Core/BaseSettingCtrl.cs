//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-12-15 18:39:25
//* 描    述：  

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Setting
{
    public abstract class BaseSettingCtrl : ISettingCtrl
    {
        private Dictionary<int, HashSet<Action<int>>> m_intEvents = new Dictionary<int, HashSet<Action<int>>>();
        private Dictionary<int, HashSet<Action<float>>> m_floatEvents = new Dictionary<int, HashSet<Action<float>>>();
        private Dictionary<int, HashSet<Action<string>>> m_stringEvents = new Dictionary<int, HashSet<Action<string>>>();
        private bool m_inExecute;
        private event Action onExecutionEndAction;
        private bool m_changed;
        public Action onSave { get; set; }

        public event Action<Exception> onException;

        public virtual void ClearAll()
        {
            m_intEvents.Clear();
            m_floatEvents.Clear();
            m_stringEvents.Clear();
            m_inExecute = false;
            onExecutionEndAction = null;
            m_changed = true;
        }

        public float GetFloat(int settingID, float defaultValue = 0)
        {
            if (TryGetFloat(settingID, out var value))
                return value;
            return defaultValue;
        }

        public int GetInt(int settingID, int defaultValue = 0)
        {
            if (TryGetInt(settingID, out var value))
                return value;
            return defaultValue;
        }

        public string GetString(int settingID, string defaultValue = "")
        {
            if (TryGetString(settingID, out var value))
                return value;
            return defaultValue;
        }

        public void RegistFloatChanged(int settingID, Action<float> callback)
        {
            if (callback == null)
                return;

            if (!m_floatEvents.TryGetValue(settingID, out var actions))
            {
                actions = new HashSet<Action<float>>();
                m_floatEvents.Add(settingID, actions);
            }

            if (!m_inExecute)
            {
                actions.Add(callback);
            }
            else
            {
                onExecutionEndAction += () => { actions.Add(callback); };
            }
        }

        public void RegistIntChanged(int settingID, Action<int> callback)
        {
            if (callback == null)
                return;

            if (!m_intEvents.TryGetValue(settingID,out var actions))
            {
                actions = new HashSet<Action<int>>();
                m_intEvents.Add(settingID, actions);
            }

            if (!m_inExecute)
            {
                actions.Add(callback);
            }
            else
            {
                onExecutionEndAction += () => { actions.Add(callback); };
            }
        }

        public void RegistStringChanged(int settingID, Action<string> callback)
        {
            if (callback == null)
                return;

            if (!m_stringEvents.TryGetValue(settingID, out var actions))
            {
                actions = new HashSet<Action<string>>();
                m_stringEvents.Add(settingID, actions);
            }

            if (!m_inExecute)
            {
                actions.Add(callback);
            }
            else
            {
                onExecutionEndAction += () => { actions.Add(callback); };
            }
        }

        public void SetFloat(int settingID, float value)
        {
            TryGetFloat(settingID, out var oldValue);

            if (oldValue != value && SetFloatOnly(settingID, value))
            {
                m_changed = true;
                if (m_floatEvents.TryGetValue(settingID,out var actions))
                {
                    m_inExecute = true;
                    using (var enumerator = actions.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            TryExecuteCallBack(enumerator.Current, value);
                        }
                    }
                    m_inExecute = false;
                    OnEndExecute();
                }
            }
        }


        public void SetInt(int settingID, int value)
        {
            TryGetInt(settingID, out var oldValue);

            if (oldValue != value && SetIntOnly(settingID, value))
            {
                m_changed = true;

                if (m_intEvents.TryGetValue(settingID, out var actions))
                {
                    m_inExecute = true;
                    using (var enumerator = actions.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            TryExecuteCallBack(enumerator.Current, value);
                        }
                    }
                    m_inExecute = false;
                    OnEndExecute();
                }
            }
        }

        public void SetString(int settingID, string value)
        {
            TryGetString(settingID, out var oldValue);

            if (oldValue != value && SetStringOnly(settingID, value))
            {
                m_changed = true;

                if (m_stringEvents.TryGetValue(settingID, out var actions))
                {
                    m_inExecute = true;
                    using (var enumerator = actions.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            TryExecuteCallBack(enumerator.Current, value);
                        }
                    }
                    m_inExecute = false;
                    OnEndExecute();
                }
            }
        }

        public abstract bool TryGetFloat(int settingID, out float value);
        protected abstract bool SetFloatOnly(int settingID, float value);

        public abstract bool TryGetInt(int settingID, out int value);
        protected abstract bool SetIntOnly(int settingID, int value);

        public abstract bool TryGetString(int settingID, out string value);
        protected abstract bool SetStringOnly(int settingID, string value);

        public void UnRegistFloatChanged(int settingID, Action<float> action)
        {
            if (action == null) return;
            if (m_floatEvents.TryGetValue(settingID, out var list))
            {
                list.Remove(action);
            }
        }

        public void UnRegistIntChanged(int settingID, Action<int> action)
        {
            if (action == null) return;
            if (m_intEvents.TryGetValue(settingID, out var list))
            {
                list.Remove(action);
            }
        }

        public void UnRegistStringChanged(int settingID, Action<string> action)
        {
            if (action == null) return;
            if (m_stringEvents.TryGetValue(settingID, out var list))
            {
                list.Remove(action);
            }
        }

        private void TryExecuteCallBack<T>(Action<T> current, T value)
        {
            try
            {
                current.Invoke(value);
            }
            catch (Exception e)
            {
                OnException(e);
            }
        }

        private void OnException(Exception e)
        {
            if (onException != null)
                onException.Invoke(e);
            else
                UnityEngine.Debug.LogException(e);
        }

        private void OnEndExecute()
        {
            if (onExecutionEndAction != null)
            {
                try
                {
                    onExecutionEndAction.Invoke();
                }
                catch (Exception e)
                {
                    OnException(e);
                }
                onExecutionEndAction = null;
            }
        }

        public virtual void FlashChange()
        {
            if(m_changed)
            {
                m_changed = false;
                DoSave();
                onSave?.Invoke();
            }
        }

        protected abstract void DoSave();
        public abstract string SerializeToJson();
        public abstract void ParseFromJson(string json);
    }
}