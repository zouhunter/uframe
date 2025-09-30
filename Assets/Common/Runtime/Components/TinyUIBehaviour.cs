/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_eba7a7                                                                *
*  功能:                                                                              *
*   - 极简UI管理器                                                                    *
*//************************************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace UFrame
{
    /// <summary>
    /// 仅保留简单的打开关闭功能
    /// </summary>
    public class TinyUIBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected Rule[] bridges;

        [System.Serializable]
        public class Rule//规则
        {
            public string from_panel;
            public string to_panel;
            public bool hide;
            public bool auto;
            public bool mutix;
        }

        [SerializeField]
        protected PanelItem[] panelItems;

        [System.Serializable]
        public class PanelItem
        {
            public string panelName;
            public GameObject prefab;
        }

        #region 固定
        protected Dictionary<string, PanelItem> itemDic;//矛盾件缓存 
        protected Dictionary<string, Rule> parentRuleBridgeDic;//父子级查找
        protected Dictionary<string, Rule> allBridgeDic;//通用规则缓存
        protected Dictionary<string, HashSet<string>> autoOpenDic;//自动打开规则
        #endregion 固定

        #region 动态改变
        protected Dictionary<int, int> parentCatchDic;//记录面板的父级(创建物体时添加)
        protected Dictionary<int, GameObject> panelCatchDic;//记录面板的GameObject;(创建物体时添加)
        protected Dictionary<string, int> instenceCatchDic;//实例对象缓存(创建物体时添加)
        protected Dictionary<int, HashSet<int>> childCatchDic;//子面板记录(创建物体时添加)
        protected Dictionary<int, HashSet<int>> hideCatchDic;//隐藏作用记录(创建物体时添加)
        protected Dictionary<int, HashSet<int>> passiveHideCatchDic;//被隐藏作用记录(创建物体时添加)
        protected Dictionary<string, HashSet<int>> mutixBridgeDic;//互斥规则缓存
        #endregion

        protected bool m_inited;

        protected void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (!m_inited)
            {
                OnInitialize();
                m_inited = true;
            }
        }

        protected virtual void OnInitialize()
        {
            InitializeHelpers();
            InitializeRuntimeDics();
        }

        /// <summary>
        /// 清空创建的预制体
        /// 释放相关字典
        /// </summary>
        public void ResetAll()
        {
            using (var enumerator = panelCatchDic.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var instence = enumerator.Current.Value;
                    if (instence)
                    {
                        Destroy(instence);
                    }
                }
            }
            instenceCatchDic.Clear();
            mutixBridgeDic.Clear();
            parentCatchDic.Clear();
            panelCatchDic.Clear();
            childCatchDic.Clear();
            hideCatchDic.Clear();
            passiveHideCatchDic.Clear();
        }

        /// <summary>
        /// 打开面板
        /// 处理父类关系
        /// 处理兄弟关系
        /// 处理子类关系
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject Open(string panel, string parent = null)
        {
            if (string.IsNullOrEmpty(panel))
                return null;

            Rule rule = TryFindRule(panel, parent);
            if (rule != null)
            {
                return FindOrInstencePanel(panel, rule.from_panel, rule.hide, rule.mutix);
            }
            else
            {
                return FindOrInstencePanel(panel);
            }

        }


        ///隐藏面板
        ///试图恢复父级
        ///释放父子关系
        ///试图打开被斥的面板
        ///释放斥锁
        /// </summary>
        /// <param name="panel"></param>
        public void Hide(string panel, bool destroy = false)
        {
            var instenceID = 0;
            if (instenceCatchDic.TryGetValue(panel, out instenceID))
            {
                HideInstence(instenceID, destroy);
                ShowBeHideItem(instenceID);
            }
        }

        public GameObject FindInstence(string panel)
        {
            GameObject go = null;
            if (instenceCatchDic.TryGetValue(panel, out int instenceID))
            {
                if (panelCatchDic.TryGetValue(instenceID, out go))
                {
                    if (go)
                        return go;
                }
            }
            return go;
        }
        public T FindComponent<T>(string panel) where T : MonoBehaviour
        {
            var instence = FindInstence(panel);
            if (instence)
            {
                return instence.GetComponent<T>();
            }
            return null;
        }

        #region protected
        protected GameObject FindOrInstencePanel(string panel, string parent = null, bool hideParent = false, bool mutix = false)
        {
            var instence = FindInstence(panel);
            if (instence == null)
            {
                instence = CreateInstence(panel);
            }

            if (instence != null)
            {
                if (!string.IsNullOrEmpty(parent))
                {
                    //处理父子关系
                    var parentInstenceID = 0;
                    if (instenceCatchDic.TryGetValue(parent, out parentInstenceID))
                    {
                        GameObject parentGo;
                        if (panelCatchDic.TryGetValue(parentInstenceID, out parentGo))
                        {
                            ProcessParentChild(instence, parentGo, hideParent);
                        }
                    }

                    //处理兄弟关系
                    if (mutix)
                    {
                        ProcessMutix(instence, parent);
                    }
                }

                if (!instence.activeSelf)
                {
                    var instenceID = instence.GetInstanceID();
                    if (CanShow(instenceID))
                    {
                        instence.gameObject.SetActive(true);
                    }
                }

                //自动打开子面板
                if (instence.activeInHierarchy)
                {
                    HashSet<string> subChilds = null;
                    if (autoOpenDic.TryGetValue(panel, out subChilds))
                    {
                        using (var enumerator = subChilds.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var childName = enumerator.Current;
                                Open(childName, panel);
                            }
                        }
                    }
                }
            }

            return instence;
        }

        /// <summary>
        /// 判断是否能显示指定的物体
        /// </summary>
        /// <param name="instenceID"></param>
        /// <returns></returns>
        protected bool CanShow(int instenceID)
        {
            bool canShow = true;
            HashSet<int> handles = null;//隐藏持有者
            if (passiveHideCatchDic.TryGetValue(instenceID, out handles) && handles.Count > 0)//被隐藏
            {
                using (var enumerator = handles.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        GameObject targetHandle = null;
                        if (panelCatchDic.TryGetValue(enumerator.Current, out targetHandle))
                        {
                            if (targetHandle != null && targetHandle.activeInHierarchy)
                            {
                                canShow = false;
                                //被显示的对象持有
                                break;
                            }
                        }
                    }
                }
            }
            return canShow;
        }

        /// <summary>
        /// 处理父子关系
        /// </summary>
        /// <param name="instence"></param>
        /// <param name="parentGo"></param>
        /// <param name="parentState"></param>
        protected void ProcessParentChild(GameObject instence, GameObject parentGo, bool hideParent)
        {
            int parentInstenceID = parentGo.GetInstanceID();
            var childInstenceID = instence.GetInstanceID();
            parentCatchDic[childInstenceID] = parentInstenceID;

            if (!childCatchDic.ContainsKey(parentInstenceID))
            {
                childCatchDic[parentInstenceID] = new HashSet<int>();
            }
            childCatchDic[parentInstenceID].Add(childInstenceID);

            if (hideParent)
            {
                parentGo.SetActive(false);
                RecordHide(childInstenceID, parentInstenceID);
            }
        }

        /// <summary>
        /// 处理同级互斥
        /// </summary>
        /// <param name="instence"></param>
        /// <param name="parent"></param>
        protected void ProcessMutix(GameObject instence, string parent)
        {
            var mutixKey = MutixKey(parent);
            if (!mutixBridgeDic.ContainsKey(mutixKey))
            {
                mutixBridgeDic[mutixKey] = new HashSet<int>();
            }
            var mutixItems = mutixBridgeDic[mutixKey];
            var instenceID = instence.GetInstanceID();
            mutixItems.Add(instenceID);

            using (var enumerator = mutixItems.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != instenceID)
                    {
                        GameObject mutixTarget = null;
                        if (panelCatchDic.TryGetValue(enumerator.Current, out mutixTarget))
                        {
                            if (mutixTarget != null)
                            {
                                mutixTarget.SetActive(false);
                                RecordHide(instenceID, enumerator.Current);
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        protected GameObject CreateInstence(string panel)
        {
            PanelItem panelItem = null;
            if (itemDic.TryGetValue(panel, out panelItem))
            {
                if (panelItem.prefab != null)
                {
                    var instence = GameObject.Instantiate(panelItem.prefab);
                    instence.transform.SetParent(transform, false);
                    instence.transform.SetAsLastSibling();
                    instence.name = panel;
                    var instenceID = instence.GetInstanceID();
                    panelCatchDic[instenceID] = instence;
                    instenceCatchDic[panel] = instenceID;
                    return instence;
                }
            }
            return null;
        }

        protected void InitializeHelpers()
        {
            allBridgeDic = new Dictionary<string, Rule>();
            parentRuleBridgeDic = new Dictionary<string, Rule>();
            autoOpenDic = new Dictionary<string, HashSet<string>>();
            for (int i = 0; i < bridges.Length; i++)
            {
                var rule = bridges[i];
                if (string.IsNullOrEmpty(rule.to_panel)) continue;

                if (!string.IsNullOrEmpty(rule.from_panel))
                {
                    parentRuleBridgeDic.Add(ParentChildKey(rule.from_panel, rule.to_panel), rule);
                }
                else
                {
                    allBridgeDic.Add(rule.to_panel, rule);
                }

                if (rule.auto && !string.IsNullOrEmpty(rule.from_panel))
                {
                    if (!autoOpenDic.ContainsKey(rule.from_panel))
                    {
                        autoOpenDic[rule.from_panel] = new HashSet<string>() { rule.to_panel };
                    }
                    else
                    {
                        autoOpenDic[rule.from_panel].Add(rule.to_panel);
                    }
                }

            }
            itemDic = new Dictionary<string, PanelItem>();
            for (int i = 0; i < panelItems.Length; i++)
            {
                var panelItem = panelItems[i];
                itemDic.Add(panelItem.panelName, panelItem);
            }
        }

        protected void InitializeRuntimeDics()
        {
            instenceCatchDic = new Dictionary<string, int>();
            mutixBridgeDic = new Dictionary<string, HashSet<int>>();
            parentCatchDic = new Dictionary<int, int>();
            panelCatchDic = new Dictionary<int, GameObject>();
            childCatchDic = new Dictionary<int, HashSet<int>>();
            hideCatchDic = new Dictionary<int, HashSet<int>>();
            passiveHideCatchDic = new Dictionary<int, HashSet<int>>();
        }

        protected string ParentChildKey(string parent, string child)
        {
            return parent + "." + child;
        }

        protected string MutixKey(string parent)
        {
            return parent;
        }
        protected Rule TryFindRule(string panel, string parent)
        {
            Rule rule = null;

            if (!string.IsNullOrEmpty(parent))
            {
                var key = ParentChildKey(parent, panel);
                if (!parentRuleBridgeDic.TryGetValue(key, out rule))//指定规则
                {
                    allBridgeDic.TryGetValue(panel, out rule);//通用规则
                }
            }
            return rule;
        }

        /// <summary>
        /// <summary>
        /// 记录隐藏索引
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="target"></param>
        protected void RecordHide(int handle, int target)
        {
            if (!hideCatchDic.ContainsKey(handle))
            {
                hideCatchDic[handle] = new HashSet<int>();
            }
            hideCatchDic[handle].Add(target);

            if (!passiveHideCatchDic.ContainsKey(target))
            {
                passiveHideCatchDic[target] = new HashSet<int>();
            }
            passiveHideCatchDic[target].Add(handle);

            //移除双向隐藏
            if (hideCatchDic.ContainsKey(target) && hideCatchDic[target].Contains(handle))
            {
                hideCatchDic[target].Remove(handle);
            }
            if (passiveHideCatchDic.ContainsKey(handle) && passiveHideCatchDic[handle].Contains(target))
            {
                passiveHideCatchDic[handle].Remove(target);
            }
        }

        /// <summary>
        /// 递归关闭面板
        /// </summary>
        /// <param name="instenceID"></param>
        protected void HideInstence(int instenceID, bool destroy)
        {
            var parentID = 0;
            if (parentCatchDic.TryGetValue(instenceID, out parentID))
            {
                childCatchDic[parentID].Remove(instenceID);
                parentCatchDic.Remove(instenceID);
            }

            //关闭所有孩子
            HashSet<int> childs = null;
            if (childCatchDic.TryGetValue(instenceID, out childs))
            {
                var childCopy = new int[childs.Count];
                childs.CopyTo(childCopy);
                for (int i = 0; i < childCopy.Length; i++)
                {
                    HideInstence(childCopy[i], destroy);
                }
                childCatchDic[instenceID].Clear();
            }

            //关闭当前面板
            GameObject panelGo = null;
            if (panelCatchDic.TryGetValue(instenceID, out panelGo) && panelGo)
            {
                if (destroy)
                {
                    GameObject.DestroyImmediate(panelGo);
                }
                else if (panelGo.activeSelf)
                {
                    panelGo.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 显示被隐藏在面板
        /// </summary>
        /// <param name="instenceID"></param>
        protected void ShowBeHideItem(int instenceID)
        {
            ///显示被锁定隐藏的物体
            HashSet<int> hidedItems = null;
            if (hideCatchDic.TryGetValue(instenceID, out hidedItems))
            {
                using (var enumerator = hidedItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var hidedItem = enumerator.Current;

                        if (passiveHideCatchDic.ContainsKey(hidedItem))
                        {
                            passiveHideCatchDic[hidedItem].Remove(instenceID);
                        }

                        if (CanShow(hidedItem))
                        {
                            GameObject body = null;
                            if (panelCatchDic.TryGetValue(hidedItem, out body))
                            {
                                if (!body.activeSelf)
                                {
                                    body.SetActive(true);
                                }
                            }
                        }
                    }

                }
                hideCatchDic.Remove(instenceID);
            }
        }

        #endregion
    }
}