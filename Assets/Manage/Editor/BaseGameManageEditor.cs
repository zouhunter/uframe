using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;

namespace UFrame.Editors
{
    [CustomEditor(typeof(BaseGameManage), true)]
    public class BaseGameManageEditor : Editor
    {
        protected ReorderableList m_fixedupdatesList;
        protected ReorderableList m_updatesList;
        protected ReorderableList m_lateupdatesList;
        protected ReorderableList m_agentsList;
        protected Dictionary<object, string> m_managerNameCatch;

        protected void OnEnable()
        {
            m_managerNameCatch = new Dictionary<object, string>();
            if (target)
            {
                BaseGameManage manage = target as BaseGameManage;
                var type = typeof(BaseGameManage);
                CreateInterfaceView(type, manage, "m_agents", ref m_agentsList);
                CreateRefreshContext("FixedUpdate", manage, "m_fixedUpdates", ref m_fixedupdatesList);
                CreateRefreshContext("Update", manage, "m_updates", ref m_updatesList);
                CreateRefreshContext("LateUpdate", manage, "m_lateUpdats", ref m_lateupdatesList);
            }
        }

        protected void CreateRefreshContext(string title,BaseGameManage gameManage, string filedName, ref ReorderableList viewList)
        {
            var manageType = typeof(BaseGameManage);
            var fileld = manageType.GetField(filedName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            if (fileld != null)
            {
                var fileldValue = fileld.GetValue(gameManage) as List<RefreshContext>;
                viewList = new ReorderableList(fileldValue, typeof(RefreshContext), false, true, false, false);
                viewList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, string.Format("[刷新{0}]", title)); };
                viewList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    RefreshContext context = fileldValue[index];
                    var manage = context.interval;
                    if (!m_managerNameCatch.TryGetValue(manage, out string managerName))
                    {
                        managerName = manage.ToString() + ":" + manage.GetType().FullName;
                    }
                    else
                    {
                        managerName = manage.GetType().FullName;
                    }

                    if (manage is IInterval)
                    {
                        var interval = (manage as IInterval);
                        managerName += "," + interval.Interval + ":" + interval.Runing;
                    }

                    EditorGUI.LabelField(rect, index.ToString() + ":" + managerName);
                };
            }
            else
            {
                Debug.LogError("can`t find :" + filedName);
            }
        }

        private void CreateInterfaceView(Type type, BaseGameManage manage, string filedName, ref ReorderableList viewList)
        {
            var fileld = type.GetField(filedName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            if (fileld != null)
            {
                var fileldValue = fileld.GetValue(manage) as System.Collections.Generic.List<AdapterBase>;
                viewList = new ReorderableList(fileldValue, typeof(AdapterBase), false, true, false, false);
                viewList.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, string.Format("[Name,Priority]", typeof(AdapterBase).FullName)); };
                viewList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var manage = fileldValue[index];
                    var managerName = manage.GetType().FullName;
                    var prior = manage.Priority;
                    EditorGUI.LabelField(rect, $"{index} : {managerName},{prior}");
                };
            }
            else
            {
                Debug.LogError("can`t find :" + filedName);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            if (m_agentsList != null && m_agentsList.count > 0)
            {
                m_agentsList.DoLayoutList();
            }
            if (m_fixedupdatesList != null && m_fixedupdatesList.count > 0)
            {
                m_fixedupdatesList.DoLayoutList();
            }
            if (m_updatesList != null && m_updatesList.count > 0)
            {
                m_updatesList.DoLayoutList();
            }
            if (m_lateupdatesList != null && m_lateupdatesList.count > 0)
            {
                m_lateupdatesList.DoLayoutList();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}