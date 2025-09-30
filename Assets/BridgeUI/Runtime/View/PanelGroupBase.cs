/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面组模板                                                                          *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public abstract class PanelGroupBase : MonoBehaviour, IPanelGroup
    {
        #region Propertys
        public UILoader bundleCreateRule;
        public Transform Trans
        {
            get { return transform; }
        }
        public UIBindingController BindingCtrl
        {
            get
            {
                return bindingCtrl;
            }
        }
        public abstract List<BridgeInfo> Bridges { get; }
        public abstract Dictionary<string, UIInfoBase> Nodes { get; }
        #endregion

        #region Protected
        protected BridgePool bridgePool = new BridgePool();
        protected List<IUIPanel> createdPanels = new List<IUIPanel>();
        protected Dictionary<string, HashSet<IUIPanel>> panelPool = new Dictionary<string, HashSet<IUIPanel>>();
        protected List<Bridge> createdBridges = new List<Bridge>();
        protected Dictionary<IUIPanel, Stack<IUIPanel>> hidedPanelStack = new Dictionary<IUIPanel, Stack<IUIPanel>>();
        protected Dictionary<int, IUIPanel> transRefDic = new Dictionary<int, IUIPanel>();//通过transform 的 id 查找UIPanel
        private PanelCreateRule _createRule;
        protected PanelCreateRule createRule
        {
            get
            {
                if (_createRule == null)
                    _createRule = new PanelCreateRule(bundleCreateRule);
                return _createRule;
            }
        }
        protected event UnityAction onDestroy;
        protected UIBindingController bindingCtrl = new UIBindingController();
        protected Dictionary<Transform, Dictionary<int, Transform>> transDicCatch = new Dictionary<Transform, Dictionary<int, Transform>>();
        #endregion

        protected virtual void OnDestroy()
        {
            if (onDestroy != null)
            {
                onDestroy.Invoke();
            }
        }

        protected void LunchPanelGroupSystem()
        {
            TryAutoOpen();
            RegistUIEvents();
        }

        public bool CreateInfoAndBridge(string panelName, IUIPanel parentPanel, int index, UIInfoBase uiInfo, out Bridge bridgeObj)
        {
            if (uiInfo == null)
            {
                bridgeObj = null;
                return false;
            }

            bridgeObj = GetBridgeClamp(parentPanel, panelName, index);
            createdBridges.Add(bridgeObj);
            return uiInfo != null && bridgeObj != null;
        }

        protected bool TryFindOldPanelBridge(string panelName, out Bridge bridgeObj)
        {
            bridgeObj = null;

            if (createdPanels == null)
                return false;

            var oldPanels = createdPanels.FindAll(x => x.Name == panelName);
            for (int i = 0; i < oldPanels.Count; i++)
            {
                var panel = oldPanels[i];
                bridgeObj = createdBridges.Find(x => x.OutPanel == panel);
                if (bridgeObj != null)
                    break;
            }
            return bridgeObj != null;
        }

        protected bool TryFindEmptyOldPanel(string panelName, out IUIPanel panel)
        {
            panel = null;

            if (createdPanels == null)
            {
                return false;
            }

            var oldPanels = createdPanels.FindAll(x => x.Name == panelName);

            for (int i = 0; i < oldPanels.Count; i++)
            {
                var panelItem = oldPanels[i];

                var bridgeItem = createdBridges.Find(x => x.OutPanel == panelItem);

                if (bridgeItem == null)
                {
                    panel = panelItem;
                    break;
                }
            }
            return panel != null;
        }

        public bool TryOpenOldPanel(IUIPanel parentPanel, Bridge bridgeObj)
        {
            if (bridgeObj != null && bridgeObj.OutPanel != null && bridgeObj.OutPanel.Target)
            {
                var oldPanel = bridgeObj.OutPanel;
                if (oldPanel.UType.form == UIFormType.Fixed)
                {
                    bridgeObj.SetInPanel(parentPanel);

                    if (parentPanel != null)
                        parentPanel.RecordChild(oldPanel);

                    oldPanel.UnHide();

                    HandBridgeOptions(oldPanel, bridgeObj);
                    return true;
                }
            }
            return false;
        }


        public Bridge FindOrCreateBridge(IUIPanel parentPanel, string panelName, int index)
        {
            UIInfoBase uiInfo;
            if (!Nodes.TryGetValue(panelName, out uiInfo))
            {
                return null;
            }
            if (TryFindOldPanelBridge(panelName, out var bridge))
            {
                bridge.SetInPanel(parentPanel);
                return bridge;
            }
            else if (CreateInfoAndBridge(panelName, parentPanel, index, uiInfo, out bridge))
            {
                return bridge;
            }
            return null;
        }

        public void Send(string panelName, object data)
        {
            Bridge bridgeObj = null;

            if (createdPanels == null)
                return;

            var oldPanels = createdPanels.FindAll(x => x.Name == panelName);
            for (int i = 0; i < oldPanels.Count; i++)
            {
                var panel = oldPanels[i];
                if (panel != null && panel.IsAlive)
                {
                    bridgeObj = createdBridges.Find(x => x.OutPanel == panel);
                    if (bridgeObj != null)
                    {
                        bridgeObj.Send(data);
                    }
                }

            }
        }

        public bool OpenPanelWithBridge(Bridge bridge)
        {
            if (TryOpenOldPanel(bridge.InPanel, bridge))
            {
                return true;
            }
            UIInfoBase uiInfo;
            if (Nodes.TryGetValue(bridge.Info.outNode, out uiInfo))
            {
                CreatePanel(uiInfo, bridge, bridge.InPanel);
                return true;
            }
            return false;
        }

        public virtual Bridge OpenPanel(IUIPanel parentPanel, string panelName, int index)
        {
            //var Content = parentPanel == null ? null : parentPanel.Content;
            UIInfoBase uiInfo;
            if (!Nodes.TryGetValue(panelName, out uiInfo))
            {
                return null;
            }

            Bridge bridge;
            if (TryFindOldPanelBridge(panelName, out bridge) && TryOpenOldPanel(parentPanel, bridge))
            {
                return bridge;
            }
            else
            {
                if (CreateInfoAndBridge(panelName, parentPanel, index, uiInfo, out bridge))
                {
                    CreatePanel(uiInfo, bridge, parentPanel);
                    return bridge;
                }
                else
                {
                    Debug.LogError("目标面板信息丢失,请检查逻辑！！！" + parentPanel);
                    for (int j = 0; j < createdBridges.Count; j++)
                    {
                        var item = createdBridges[j];
                        Debug.Log(item.Info.outNode);
                    }
                }
            }

            return null;
        }

        public void ClosePanel(string panelName)
        {
            var panels = RetrivePanels(panelName);
            if (panels != null)
            {
                foreach (var panel in panels)
                {
                    panel.Close();
                }
            }
            CansaleInstencePanel(panelName);
        }

        public void HidePanel(string panelName)
        {
            var panels = RetrivePanels(panelName);
            if (panels != null)
            {
                foreach (var panel in panels)
                {
                    panel.Hide();
                }
            }
            CansaleInstencePanel(panelName);
        }

        public void UnHidePanel(string panelName)
        {
            var panels = RetrivePanels(panelName);
            if (panels != null)
            {
                foreach (var panel in panels)
                {
                    panel.UnHide();
                }
            }
        }

        public bool IsHavePanel(string panelName)
        {
            var panels = RetrivePanels(panelName);
            if ((panels != null && panels.Count > 0))
            {
                return true;
            }
            return false;
        }

        public bool IsPanelOpen(string panelName, bool includeHide = false)
        {
            var panels = RetrivePanels(panelName);
            if ((panels != null && panels.Count > 0))
            {
                foreach (var panel in panels)
                {
                    if (includeHide && panel.IsAlive)
                        return true;
                    else if (panel.IsAlive && panel.IsShowing)
                        return true;
                }
            }
            return false;
        }

        public bool IsPanelOpen<T>(string panelName, out T[] panels, bool includeHide = false) where T : IUIPanel
        {
            var obj_panels = RetrivePanels(panelName, includeHide);
            if (obj_panels != null && obj_panels.Count > 0)
            {
                panels = obj_panels.ToArray() as T[];
                return true;
            }
            panels = null;
            return false;
        }

        public void CreatePanel(UIInfoBase uiNode, Bridge bridge, IUIPanel parentPanel)
        {
            Transform root = parentPanel == null ? Trans : parentPanel.GetContent(bridge.Info.index);

            var createUIHandle = new UICreateHandle();
            createUIHandle.parentPanel = parentPanel;
            createUIHandle.uiNode = uiNode;
            createUIHandle.bridge = bridge;
            createUIHandle.parent = root;
            createUIHandle.panel = FindUIPanelFromPool(uiNode.panelName);
            createUIHandle.onCreate = CreateUIInternal;
            if (createUIHandle.panel != null && createUIHandle.panel.Target)
                createUIHandle.OnCreate(createUIHandle.panel.Target);
            else
                createRule.CreatePanel(uiNode, createUIHandle.OnCreate);
        }


        protected void CreateUIInternal(GameObject go, UIInfoBase uiNode, Bridge bridge, Transform parent, IUIPanel parentPanel, IUIPanel panel)
        {
            if (go == null) return;

            var parentDic = GetParentDic(parent);

            Utility.SetTranform(go.transform, uiNode.type.layer, uiNode.type.layerIndex, parent, parentDic, transRefDic);

            go.name = uiNode.panelName;
            go.SetActive(true);

            if (panel == null)
                panel = CreateUIPanel(go);
            InitPanelInformation(panel, uiNode);
            panel.Binding(go);
            panel.HandleData(bridge);
            transRefDic[go.transform.GetInstanceID()] = panel;
            createdPanels.Add(panel);

            if (parentPanel != null)
            {
                parentPanel.RecordChild(panel);
            }

            if (bridge != null)
            {
                bridge.OnCreatePanel(panel);
                bridge.OnToggleHidePanel(false);
            }

            HandBridgeOptions(panel, bridge);
        }

        private IUIPanel FindUIPanelFromPool(string panelName)
        {
            IUIPanel panel = null;
            if (panelPool.TryGetValue(panelName, out var panels))
            {
                using (var enumerator = panels.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        panel = enumerator.Current;
                        panels.Remove(panel);
                        break;
                    }
                }
            }
            return panel;
        }

        public void CansaleInstencePanel(string panelName)
        {
            createRule.CansaleCreatePanel(panelName);
        }

        public List<IUIPanel> RetrivePanels(string panelName, bool includeHide = true)
        {
            var panels = createdPanels.FindAll(x => x.Name == panelName);
            if (!includeHide)
                panels.RemoveAll(x => !x.IsShowing);
            return panels;
        }

        #region protected Functions

        /// <summary>
        /// 处理面板打开规则
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="bridge"></param>
        protected void HandBridgeOptions(IUIPanel panel, Bridge bridge)
        {
            Debug.Assert(bridge != null, "信息不应当为空，请检查！");
            TryChangeParentState(panel, bridge.Info);
            TryHandleMutexPanels(panel, bridge.Info);
            TryHideGroup(panel, bridge.Info);
            TryAutoOpen(panel);
        }

        /// <summary>
        /// 隐藏整个面板中其他的ui界面
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="bridge"></param>
        protected void TryHideGroup(IUIPanel panel, BridgeInfo bridge)
        {
            if ((bridge.showModel.single))
            {
                var parent = createdPanels.Find(x => x.Name == bridge.inNode);
                if (parent != null)
                {
                    var parentDic = GetParentDic(Trans);
                    panel.SetParent(Trans, parentDic, transRefDic);
                }
                foreach (var oldPanel in createdPanels)
                {
                    if (oldPanel != panel)
                    {
                        HidePanelInteral(panel, oldPanel);
                    }
                }
            }
        }

        /// <summary>
        /// 互斥面板自动隐藏
        /// </summary>
        /// <param name="childPanel"></param>
        /// <param name=""></param>
        /// <param name="bridge"></param>
        protected void TryHandleMutexPanels(IUIPanel childPanel, BridgeInfo bridge)
        {
            if (bridge.showModel.mutex != MutexRule.NoMutex)
            {
                if (bridge.showModel.mutex == MutexRule.SameParentAndLayer)
                {
                    var mayBridges = Bridges.FindAll(x => x.inNode == bridge.inNode);

                    foreach (var bg in mayBridges)
                    {
                        if (bg.showModel.mutex != MutexRule.SameParentAndLayer) continue;

                        var mayPanels = createdPanels.FindAll(x =>
                        x.Name == bg.outNode &&
                        x.UType.layer == childPanel.UType.layer &&
                        x != childPanel &&
                        !IsChildOfPanel(childPanel, x));

                        foreach (var mayPanel in mayPanels)
                        {
                            if (mayPanel != null && mayPanel.IsShowing)
                            {
                                if (mayPanel.UType.layerIndex > childPanel.UType.layerIndex)
                                {
                                    HidePanelInteral(mayPanel, childPanel);
                                }
                                else
                                {
                                    HidePanelInteral(childPanel, mayPanel);
                                }
                            }
                        }

                    }
                }
                else if (bridge.showModel.mutex == MutexRule.SameLayer)
                {
                    var mayPanels = createdPanels.FindAll(x => x.UType.layer == childPanel.UType.layer && x != childPanel && !IsChildOfPanel(childPanel, x));
                    foreach (var mayPanel in mayPanels)
                    {
                        if (mayPanel != null && mayPanel.IsShowing)
                        {
                            if (mayPanel.UType.layerIndex > childPanel.UType.layerIndex)
                            {
                                HidePanelInteral(mayPanel, childPanel);
                            }
                            else
                            {
                                HidePanelInteral(childPanel, mayPanel);
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 判断面板的父子关系
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected bool IsChildOfPanel(IUIPanel current, IUIPanel target)
        {
            if (current.Parent == null)
            {
                return false;
            }
            if (current.Parent == target)
            {
                return true;
            }
            else
            {
                return IsChildOfPanel(current.Parent, target);
            }
        }

        ///  <summary>
        /// 自动打开子面板
        /// </summary>
        /// <param name="content"></param>
        /// <param name="parentPanel"></param>
        protected void TryAutoOpen(IUIPanel parentPanel = null)
        {
            var panelName = parentPanel == null ? "" : parentPanel.Name;
            var autoBridges = Bridges.FindAll(x => CompareName(x.inNode, panelName) && x.showModel.auto);
            if (autoBridges != null)
            {
                foreach (var autoBridge in autoBridges)
                {
                    panelName = autoBridge.outNode;

                    UIInfoBase uiNode;

                    Nodes.TryGetValue(panelName, out uiNode);

                    if (uiNode == null)
                    {
                        Debug.LogError("无配制信息：" + panelName);
                        continue;
                    }

                    Bridge bridge;
                    if (!TryFindOldPanelBridge(panelName, out bridge) || !TryOpenOldPanel(parentPanel, bridge))
                    {
                        if (CreateInfoAndBridge(panelName, parentPanel, -1, uiNode, out bridge))
                        {
                            CreatePanel(uiNode, bridge, parentPanel);
                        }
                        else
                        {
                            Debug.LogError("找不到信息：" + panelName);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 名称比较
        /// </summary>
        /// <param name="nameA"></param>
        /// <param name="nameB"></param>
        /// <returns></returns>
        protected bool CompareName(string nameA, string nameB)
        {
            if (string.IsNullOrEmpty(nameA))
            {
                return string.IsNullOrEmpty(nameB);
            }
            return string.Compare(nameA, nameB) == 0;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="needHidePanel"></param>
        protected void HidePanelInteral(IUIPanel panel, IUIPanel needHidePanel)
        {
            if (needHidePanel.IsShowing)
            {
                needHidePanel.Hide();
            }
            if (!hidedPanelStack.ContainsKey(panel))
            {
                hidedPanelStack[panel] = new Stack<IUIPanel>();
            }
            //Debug.Log("push:" + needHidePanel);
            hidedPanelStack[panel].Push(needHidePanel);
        }

        /// <summary>
        /// 按规则设置面板及父亲面板的状态
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="bridge"></param>
        /// <param name="uiNode"></param>
        protected void InitPanelInformation(IUIPanel panel, UIInfoBase uiNode)
        {
            UIType uType = uiNode.type;
            panel.Group = this;
            panel.onClose -= OnDeletePanel;
            panel.onClose += OnDeletePanel;
            panel.onHide -= OnHidePanel;
            panel.onHide += OnHidePanel;
            panel.onShow -= OnShowPanel;
            panel.onShow += OnShowPanel;
            panel.UType = uType;
        }
        /// <summary>
        /// 选择性隐藏父级
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="bridge"></param>
        protected void TryChangeParentState(IUIPanel panel, BridgeInfo bridge)
        {
            if (bridge.showModel.baseShow == ParentShow.Hide)
            {
                var parent = panel.Parent;

                if (parent != null)
                {
                    var parentDic = GetParentDic(parent.Root);
                    panel.SetParent(parent.Root, parentDic, transRefDic);
                    HidePanelInteral(panel, parent);
                }
            }

            if (bridge.showModel.baseShow == ParentShow.Destroy)
            {
                var parent = panel.Parent;
                if (parent != null && parent.ChildPanels.Count > 0)
                {
                    var parentDic = GetParentDic(parent.Root);
                    panel.SetParent(parent.Root, parentDic, transRefDic);
                    parent.ChildPanels.Remove(panel);

                    if (hidedPanelStack.ContainsKey(parent))
                    {
                        if (!hidedPanelStack.ContainsKey(panel))
                        {
                            hidedPanelStack[panel] = new Stack<IUIPanel>();
                        }
                        while (hidedPanelStack[parent].Count > 0)
                        {
                            hidedPanelStack[panel].Push(hidedPanelStack[parent].Pop());
                        }
                    }

                    parent.Close();
                }
            }
        }

        /// <summary>
        /// 获取可用的bridge
        /// </summary>
        /// <param name="parentPanel"></param>
        /// <param name="panelName"></param>
        /// <returns></returns>
        protected Bridge GetBridgeClamp(IUIPanel parentPanel, string panelName, int index)
        {
            Bridge bridge = null;
            var parentName = parentPanel == null ? "" : parentPanel.Name;
            var mayInfos = Bridges.FindAll(x => x.outNode == panelName && (x.index == index || index == -1));//所有可能的
            var baseInfos = mayInfos.FindAll(x => x.inNode == parentName);//所有父级名相同的
            BridgeInfo bridgeInfo = null;
            if (baseInfos.Count > 0)
            {
                bridgeInfo = baseInfos[0];
            }
            else
            {
                var usefulInfos = mayInfos.FindAll(x => string.IsNullOrEmpty(x.inNode));
                if (usefulInfos.Count > 0)
                {
                    bridgeInfo = usefulInfos[0];
                }
            }

            if (bridgeInfo == null)
            {
                var show = new ShowMode();
                var info = new BridgeInfo(parentName, panelName, show, 0);
                bridge = bridgePool.Allocate(info, parentPanel);
            }
            else
            {
                bridge = bridgePool.Allocate((BridgeInfo)bridgeInfo, parentPanel);
            }
            return bridge;
        }

        /// <summary>
        /// 当界面隐藏显示变更时
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="hide"></param>
        protected void OnHidePanel(IUIPanel panel)
        {
            var bridge = createdBridges.Find(x => x.OutPanel == panel);
            if (bridge != null)
            {
                bridge.OnToggleHidePanel(true);
            }
        }

        /// <summary>
        /// 当界面隐藏显示变更时
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="hide"></param>
        protected void OnShowPanel(IUIPanel panel)
        {
            var bridge = createdBridges.Find(x => x.OutPanel == panel);
            if (bridge != null)
            {
                bridge.OnToggleHidePanel(false);
            }
        }

        /// <summary>
        /// 当删除一个面板时触发一些事
        /// </summary>
        /// <param name="panel"></param>
        protected void OnDeletePanel(IUIPanel panel)
        {
            if (createRule != null)
                createRule.ReleasePanel(panel.Name);

            //移到缓存池
            RecoverPanel(panel);

            //移除id字典
            if (panel.UType.closeRule != CloseRule.HideGameObject)
            {
                //移除连通器
                createdBridges.RemoveAll(x => x.OutPanel == panel);

                if (panel.Content && panel.Content != Trans)
                {
                    transDicCatch.Remove(panel.Content);
                }

                transRefDic?.Remove(panel.InstenceID);
            }

            //处理当前界面不可见逻辑
            ProcessOnPanelInVisiable(panel);
        }

        protected void ProcessOnPanelInVisiable(IUIPanel panel)
        {
            //关闭子面板
            if (panel.ChildPanels != null)
            {
                var childs = panel.ChildPanels.ToArray();
                foreach (var item in childs)
                {
                    if (item.IsAlive)
                    {
                        item.Close();
                    }
                }
                panel.ChildPanels.Clear();
            }

            //显示隐藏面板
            if (hidedPanelStack.ContainsKey(panel))
            {
                var mayactive = new List<IUIPanel>();
                var stack = hidedPanelStack[panel];
                if (stack != null)
                {
                    while (stack.Count > 0)
                    {
                        var item = stack.Pop();
                        mayactive.Add(item);
                    }
                }
                hidedPanelStack.Remove(panel);
                TryOpenPanels(mayactive.ToArray());
            }
        }

        /// <summary>
        /// 尝试打开隐藏的面板
        /// （如果没有被占用，则可以打开）
        /// </summary>
        /// <param name="panels"></param>
        protected void TryOpenPanels(IUIPanel[] panels)
        {
            bool canActive = true;
            foreach (var item in panels)
            {
                canActive = true;
                foreach (var panelStack in hidedPanelStack)
                {
                    if (panelStack.Value.Contains(item))
                    {
                        canActive = false;
                        break;
                    }
                }

                if (canActive && item.IsAlive && !item.IsShowing)
                {
                    item.UnHide();
                }
            }
        }

        /// <summary>
        /// 从对象池或创建代码
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IUIPanel CreateUIPanel(GameObject target)
        {
            IUIPanel panel = null;
            var panelRef = target.GetComponent<IClassReference>();
            if (panelRef != null)
            {
                var type = panelRef.LoadViewScriptType();
                if (type != null)
                {
                    if (typeof(SubView).IsAssignableFrom(type))
                    {
                        panel = new RuntimeViewBase();
                    }
                    else if (typeof(IUIPanel).IsAssignableFrom(type))
                    {
                        panel = System.Activator.CreateInstance(type) as IUIPanel;
                    }
                }
            }
            if (panel == null)
            {
                panel = new ViewBase_Diffuse();
            }
            return panel;
        }

        /// <summary>
        /// 回收面板脚本
        /// </summary>
        /// <param name="panel"></param>
        public void RecoverPanel(IUIPanel panel)
        {
            if (createdPanels.Contains(panel))
            {
                createdPanels.Remove(panel);
                var panelName = panel.Name;
                if (!panelPool.TryGetValue(panelName, out var panels))
                {
                    panels = panelPool[panelName] = new HashSet<IUIPanel>();
                }
                panels.Add(panel);
            }
        }
        protected Dictionary<int, Transform> GetParentDic(Transform parent)
        {
            if (parent == null) return null;

            if (!transDicCatch.ContainsKey(parent))
            {
                transDicCatch[parent] = new Dictionary<int, Transform>();
            }

            return transDicCatch[parent];
        }

        #region 图形化界面关联
        /// <summary>
        /// 注册ui事件
        /// </summary>
        protected void RegistUIEvents()
        {

            foreach (var item in Bridges)
            {
                var bridgeInfo = item;

                if (!string.IsNullOrEmpty(bridgeInfo.inNode) && !string.IsNullOrEmpty(bridgeInfo.outNode))
                {
                    UIBindingItem bindingItem = new UIBindingItem();

                    var index = item.index;

                    bindingItem.openAction = (x, y) =>
                    {
                        var parentPanel = x;
                        var panelName = bridgeInfo.outNode;
                        var bridge = this.OpenPanel(parentPanel, panelName, index);
                        if (bridge != null)
                        {
                            bridge.Send(y);
                        }
                    };

                    bindingItem.closeAction = () =>
                    {
                        var panelName = bridgeInfo.outNode;
                        ClosePanel(panelName);
                    };

                    bindingItem.hideAction = () =>
                    {
                        var panelName = bridgeInfo.outNode;
                        HidePanel(panelName);
                    };

                    bindingItem.isOpenAction = () =>
                    {
                        var panelName = bridgeInfo.outNode;
                        return IsPanelOpen(panelName);
                    };

                    bindingCtrl.RegistPanelEvent(bridgeInfo.inNode, bridgeInfo.index, bindingItem);

                    this.onDestroy += () =>
                    {
                        //在本组合关闭时销毁事件
                        bindingCtrl.RemovePanelEvent(bridgeInfo.inNode, bridgeInfo.index, bindingItem);
                    };
                }

            }
        }

        #endregion


        #endregion protected Functions


        [ContextMenu("Make Layers")]
        void CreateLayerParents()
        {
            var layerType = typeof(UILayerType);
            var layerNames = System.Enum.GetNames(layerType);
            var layer = LayerMask.NameToLayer("UI");
            bool haveCanvas = transform.GetComponentInParent<Canvas>();
            var cameras = GameObject.FindObjectsOfType<Camera>();
            Camera uiCamera = null;
            if (cameras != null && cameras.Length > 0)
            {
                uiCamera = System.Array.Find(cameras, x => x.name == "UICamera");
                if (!uiCamera)
                    uiCamera = cameras[0];
            }
            for (int i = 0; i < layerNames.Length; i++)
            {
                var layerName = layerNames[i];
                var layerValue = (int)(UILayerType)System.Enum.Parse(layerType, layerName);
                var childName = string.Format("{0}|{1}", layerValue, layerName);
                var layerContent = transform.Find(childName);
                if (layerContent == null)
                {
                    if (transform is RectTransform)
                    {
                        var rectParent = new GameObject(childName, typeof(RectTransform)).GetComponent<RectTransform>();
                        rectParent.SetParent(transform, false);
                        rectParent.anchorMin = Vector2.zero;
                        rectParent.anchorMax = Vector2.one;
                        rectParent.offsetMin = Vector3.zero;
                        rectParent.offsetMax = Vector3.zero;
                        rectParent.anchoredPosition = Vector2.zero;
                        layerContent = rectParent;
                    }
                    else
                    {
                        layerContent = new GameObject(childName).transform;
                        layerContent.SetParent(transform, true);
                    }
                    layerContent.gameObject.layer = layer;

                    if (!haveCanvas)
                    {
                        var layerCanvas = layerContent.gameObject;
                        var canvas = layerCanvas.AddComponent<Canvas>();
                        var scaler = layerCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                        layerCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.sortingOrder = i;
                        canvas.worldCamera = uiCamera;
                        canvas.planeDistance = 100 - i * 20;
                        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920, 1080);
                    }
                }
            }
        }
    }
}