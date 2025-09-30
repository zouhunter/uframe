using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UFrame.BridgeUI
{
    [PanelParent]
    public class ViewBase : IUIPanel
    {
        #region Propertys
        public GameObject Target { get { return target; } }
        public int InstenceID => m_instanceId;
        public string Name { get { return name; } }
        public virtual Transform Content { get { return Group.Trans; } }
        private int m_instanceId;
        public IPanelGroup Group
        {
            get
            {
                if (group == null)
                    group = target.GetComponentInParent<IPanelGroup>();
                return group;
            }
            set { group = value; }
        }
        public IUIPanel Parent { get; set; }
        public Transform Root { get { return target.transform.parent.parent; } }
        public UIType UType { get; set; }
        public List<IUIPanel> ChildPanels
        {
            get
            {
                return childPanels;
            }
        }
        public bool IsShowing
        {
            get
            {
                return target && target.activeSelf && !m_hide;
            }
        }
        public bool IsAlive
        {
            get
            {
                return target && viewBaseBinding;
            }
        }
        #endregion

        protected GameObject target { get; private set; }
        protected IPanelGroup group;
        protected Bridge bridge;
        protected List<IUIPanel> childPanels;
        protected event UnityAction<object> onReceive;
        public event System.Action<IUIPanel> onShow;
        public event System.Action<IUIPanel> onClose;
        public event System.Action<IUIPanel> onHide;

        protected Dictionary<int, Transform> childDic;
        protected List<IUIControl> uiControls;
        protected DestroyMonitor viewBaseBinding;
        protected bool m_hide;
        private string name;
        protected bool initialzed;
        protected List<KeyValuePair<List<UnityEngine.EventSystems.EventTrigger.Entry>, UnityEngine.EventSystems.EventTrigger.Entry>> m_registedList;

        #region BridgeAPI

        public void Binding(GameObject target)
        {
            //Debug.LogError(this.GetHashCode() + "Binding:" +target.GetInstanceID().ToString());
            if (IsAlive)
            {
                OnUnBinding();
            }
            if (target != null)
            {
                m_instanceId = target.GetInstanceID();
                OnBinding(target);
                AppendComponentsByType();
                OnOpenInternal();
                TryMakeCover();
            }
            else
            {
                Debug.LogError("Binding(GameObject target) empty!");
            }
        }

        public void RegistOnRecevie(UnityAction<object> onReceive)
        {
            this.onReceive += onReceive;
        }

        public void RemoveOnRecevie(UnityAction<object> onReceive)
        {
            this.onReceive += onReceive;
        }

        public void SetParent(Transform Trans, Dictionary<int, Transform> transDic, Dictionary<int, IUIPanel> transRefDic)
        {
            Utility.SetTranform(target.transform, UType.layer, UType.layerIndex, Trans, transDic, transRefDic);
        }

        public void CallBack(object data)
        {
            if (bridge != null)
            {
                bridge.CallBack(this, data);
            }
        }

        public void HandleData(Bridge bridge)
        {
            this.bridge = bridge;
            if (bridge != null)
            {
                HandleData(bridge.dataQueue);
                bridge.onGet = HandleData;
            }
        }

        public void Hide()
        {
            m_hide = true;
            switch (UType.hideRule)
            {
                case HideRule.AlaphGameObject:
                    OnHide();
                    break;
                case HideRule.HideGameObject:
                    target?.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void UnHide()
        {
            if(IsShowing)
            {
                OnRepeatOpen();
                return;
            }
            m_hide = false;
            if (target)
            {
                if(UType.hideRule == HideRule.AlaphGameObject)
                    OnShow();
                else
                    target.SetActive(true);
            }
            else
            {
                Debug.LogError("target is empty!");
            }
        }

        public virtual void Close()
        {
            if (target == null)
            {
                Debug.LogError(this.GetHashCode() + " target empty!");
                return;
            }
            switch (UType.closeRule)
            {
                case CloseRule.DestroyImmediate:
                    Object.DestroyImmediate(target);
                    break;
                case CloseRule.DestroyDely:
                    Object.Destroy(target, 0.02f);
                    break;
                case CloseRule.DestroyNoraml:
                    Object.Destroy(target);
                    break;
                case CloseRule.HideGameObject:
                    target.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void RecordChild(IUIPanel childPanel)
        {
            if (childPanels == null)
            {
                childPanels = new List<IUIPanel>();
            }
            if (!childPanels.Contains(childPanel))
            {
                childPanel.onClose += OnRemoveChild;
                childPanels.Add(childPanel);
            }
            childPanel.Parent = this;
        }

        public Image Cover()
        {
            var covername = Name + "_Cover";
            var rectt = new GameObject(covername, typeof(RectTransform)).GetComponent<RectTransform>();
            rectt.gameObject.layer = 5;
            rectt.SetParent(target.transform, false);
            rectt.SetSiblingIndex(0);
            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10000);
            rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10000);
            var img = rectt.gameObject.AddComponent<Image>();
            img.color = UType.maskColor;
            img.raycastTarget = true;
            return img;
        }
        #endregion

        #region Extend Of Open Close
        public void Open(string panelName, object data = null)
        {
            var bridge = Group.OpenPanel(this, panelName, -1);
            if (bridge != null)
            {
                bridge.Send(data);
            }
        }

        public void Open(int index, object data = null)
        {
            Group.BindingCtrl.OpenRegistedPanel(this, index, data);
        }
        public void Hide(string panelName)
        {
            Group.HidePanel(panelName);
        }
        public void Hide(int index)
        {
            Group.BindingCtrl.HideRegistedPanel(this, index);
        }

        public void Close(string panelName)
        {
            Group.ClosePanel(panelName);
        }
        public void Close(int index)
        {
            Group.BindingCtrl.CloseRegistedPanel(this, index);
        }
        public bool IsOpen(int index)
        {
            return Group.BindingCtrl.IsRegistedPanelOpen(this, index);
        }
        public bool IsOpen(string panelName)
        {
            var panels = Group.RetrivePanels(panelName);
            return (panels != null && panels.Count > 0);
        }
        #endregion

        #region Protected
        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="target"></param>
        protected virtual void OnBinding(GameObject target)
        {
            this.target = target;
            name = target.name;
            viewBaseBinding = this.target.GetComponent<DestroyMonitor>();
            if (!viewBaseBinding)
                viewBaseBinding = target.AddComponent<DestroyMonitor>();
            viewBaseBinding.onDestroy = OnTargetDestroy;
            viewBaseBinding.onDisable = OnHide;
            viewBaseBinding.onEnable = OnShow;
        }
        /// <summary>
        /// 去除绑定
        /// </summary>
        /// <param name="target"></param>
        protected virtual void OnUnBinding()
        {
            if (this.target)
            {
                viewBaseBinding = this.target.GetComponent<DestroyMonitor>();
                if (viewBaseBinding)
                {
                    viewBaseBinding.onDestroy = null;
                }
            }
            this.target = null;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInitialize()
        {
            if(!initialzed)
            {
                InitializeChildControls();
                initialzed = true;
            }
            else
            {
                Debug.LogError($"View:{Name} Initialized!");
            }
        }

        /// <summary>
        /// 1.(关闭，丢失)删除对象
        /// 2.(关闭)隐藏对象
        /// </summary>
        protected virtual void OnRelease()
        {
            if(initialzed)
            {
                if (bridge != null)
                    bridge.Release();
                bridge = null;
                RemoveAllRegistedEvents();
                OnReleaseChildContrls();
                initialzed = false;
            }
            else
            {
                Debug.LogError($"View:{Name} not Initialize or Released!");
            }
        }

        public virtual Transform GetContent(int index)
        {
            return Content;
        }

        protected void OnTargetDestroy()
        {
            OnRelease();
            OnUnBinding();
            OnClose();
        }

        protected virtual void OnClose()
        {
            onClose?.Invoke(this);
        }

        protected virtual void OnHide()
        {
            m_hide = true;
            if (UType.hideRule == HideRule.AlaphGameObject)
                AlaphGameObject(m_hide);
            onHide?.Invoke(this);
        }

        protected virtual void OnShow()
        {
            m_hide = false;
            onShow?.Invoke(this);
        }

        protected void HandleData(Queue<object> dataQueue)
        {
            if (dataQueue != null)
            {
                while (dataQueue.Count > 0)
                {
                    var data = dataQueue.Dequeue();
                    if (data != null)
                    {
                        HandleData(data);
                    }
                }
            }
        }

        protected virtual void HandleData(object data)
        {
            if (this.onReceive != null)
            {
                onReceive.Invoke(data);
            }
        }

        protected void OnOpenInternal()
        {
            OnInitialize();
            OnShow();
        }

        protected void AppendComponentsByType()
        {
            if (UType.form == UIFormType.DragAble)
            {
                if (target.GetComponent<DragPanel>() == null)
                {
                    target.AddComponent<DragPanel>();
                }
            }
        }

        protected void OnRemoveChild(IUIPanel childPanel)
        {
            if (childPanels != null && childPanels.Contains(childPanel))
            {
                childPanels.Remove(childPanel);
            }
        }
        protected virtual void OnRepeatOpen()
        {

        }
        /// <summary>
        /// 建立遮罩
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="info"></param>
        protected void TryMakeCover()
        {
            switch (UType.cover)
            {
                case UIMask.None:
                    break;
                case UIMask.Normal:
                    Cover();
                    break;
                case UIMask.ClickClose:
                    var img = Cover();
                    var btn = img.gameObject.AddComponent<Button>();
                    btn.onClick.AddListener(Close);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 将面板透明处理
        /// </summary>
        /// <param name="hide"></param>
        protected void AlaphGameObject(bool hide)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            if (hide)
            {
                canvasGroup.alpha = UType.hideAlaph;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// 注册UI子控件
        /// </summary>
        /// <param name="control"></param>
        protected void RegistUIControl(BridgeUI.IUIControl control)
        {
            if (control == null) return;

            if (this.uiControls == null)
            {
                uiControls = new List<IUIControl>() { control };
            }
            else if (!uiControls.Contains(control))
            {
                uiControls.Add(control);
                if (!control.Initialized)
                    control.Initialize(this);
            }
        }
        /// <summary>
        /// 移除子控件
        /// </summary>
        /// <param name="control"></param>
        protected void RemoveUIControl(BridgeUI.IUIControl control)
        {
            if (control == null) return;

            if (this.uiControls != null)
            {
                uiControls.Remove(control);
            }
        }
        /// <summary>
        /// 绑定子面板
        /// </summary>
        protected T BindingSubReference<T>(BindingReference subReference) where T : SubView, new()
        {
            var subPanel = new T();
            subPanel.Binding(subReference);
            RegistUIControl(subPanel);
            return subPanel;
        }

        /// <summary>
        /// 解绑定子面板
        /// </summary>
        /// <param name="subPanel"></param>
        protected void UnBindingSubReference(SubView subPanel)
        {
            if (subPanel != null)
            {
                subPanel.UnBinding();
                RemoveUIControl(subPanel);
            }
        }
        #endregion

        #region Private Functions
        private void InitializeChildControls()
        {
            if (uiControls != null)
            {
                using (var enumerator = uiControls.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Initialized)
                        {
                            enumerator.Current.Initialize(this);
                        }
                    }
                }
            }
        }

        private void OnReleaseChildContrls()
        {
            if (uiControls != null)
            {
                using (var enumerator = uiControls.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.Initialized)
                        {
                            enumerator.Current.Release();
                        }
                    }
                }
                uiControls.Clear();
            }
        }
        #endregion Private Functions

        #region EventSystems
        public void RegistEvent(Component target, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callBack)
        {
            if (target)
            {
                RegistEvent(target.gameObject, type, callBack);
            }
        }
        public void RegistEvent(GameObject target, EventTriggerType type, UnityAction<BaseEventData> callBack)
        {
            if (target)
            {
                if (m_registedList == null)
                    m_registedList = new List<KeyValuePair<List<EventTrigger.Entry>, EventTrigger.Entry>>();

                var eventTrigger = target.GetComponent<EventTrigger>();
                if (!eventTrigger)
                    eventTrigger = target.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = type;
                entry.callback.AddListener(callBack);
                eventTrigger.triggers.Add(entry);
                m_registedList.Add(new KeyValuePair<List<EventTrigger.Entry>, EventTrigger.Entry>(eventTrigger.triggers,entry));
            }
        }
        protected void RemoveAllRegistedEvents()
        {
            if(m_registedList != null)
            {
                foreach (var pair in m_registedList)
                {
                    if (pair.Key != null && pair.Value != null)
                        pair.Key.Remove(pair.Value);
                }
                m_registedList.Clear();
            }
        }
        public void RegistClick(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerClick, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistClick(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerClick, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistEnter(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerEnter, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistEnter(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerEnter, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerExit(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerExit, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerExit(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerExit, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerDown(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerDown, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerDown(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerDown, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerUp(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerUp, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistPointerUp(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.PointerUp, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistBeginDrag(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.BeginDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistBeginDrag(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.BeginDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistDrag(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Drag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistDrag(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Drag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistEndDrag(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.EndDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistEndDrag(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.EndDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistCancel(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Cancel, callback);
        }
        public void RegistCancel(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Cancel, callback);
        }
        public void RegistDeselect(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Deselect, callback);
        }
        public void RegistDeselect(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Deselect, callback);
        }
        public void RegistDrop(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Drop, callback);
        }
        public void RegistDrop(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Drop, callback);
        }
        public void RegistInitializePotentialDrag(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.InitializePotentialDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistInitializePotentialDrag(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.InitializePotentialDrag, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistMove(GameObject target, UnityAction<AxisEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Move, (data) => { callback?.Invoke(data as AxisEventData); });
        }
        public void RegistMove(Component target, UnityAction<AxisEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Move, (data) => { callback?.Invoke(data as AxisEventData); });
        }
        public void RegistScroll(GameObject target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Scroll, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistScroll(Component target, UnityAction<PointerEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Scroll, (data) => { callback?.Invoke(data as PointerEventData); });
        }
        public void RegistSelect(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Select, callback);
        }
        public void RegistSelect(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Select, callback);
        }
        public void RegistSubmit(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Submit, callback);
        }
        public void RegistSubmit(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.Submit, callback);
        }
        public void RegistUpdateSelected(GameObject target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.UpdateSelected, callback);
        }
        public void RegistUpdateSelected(Component target, UnityAction<BaseEventData> callback)
        {
            RegistEvent(target, EventTriggerType.UpdateSelected, callback);
        }
        #endregion

    }
}