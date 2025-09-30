using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace UINodePanel
{
    public sealed class NodePanel : MonoBehaviour
    {
        public Button closeBtn;
        public OpenType openType;
        public List<NodePanelPair> relatedPanels;
        private UnityAction onClose;
        private void Awake()
        {
            if (openType == OpenType.ByName || openType == OpenType.ByButton){
                if(closeBtn) closeBtn.onClick.AddListener(Close);
            }
            for (int i = 0; i < relatedPanels.Count; i++)
            {
                NodePanelPair pair = relatedPanels[i];
                if ((pair.nodePanel.openType & OpenType.ByButton) == OpenType.ByButton)
                {
                    if (pair.openBtn != null) pair.openBtn.onClick.AddListener(() =>
                    {
                        if (pair.hideSelf)
                        {
                            if (pair.nodePanel.Open(() => { UnHide();}))
                            {
                                Hide();
                            }
                        }
                        else
                        {
                            pair.nodePanel.UnHide();
                        }
                    });
                }
                if ((pair.nodePanel.openType & OpenType.ByToggle) == OpenType.ByToggle)
                {
                    if (pair.openTog != null) pair.openTog.onValueChanged.AddListener((x) =>
                    {
                        if (x)
                        {
                            pair.nodePanel.UnHide();
                        }
                        else
                        {
                            pair.nodePanel.Close();
                        }
                    });
                }
                if ((pair.nodePanel.openType & OpenType.ByName) == OpenType.ByName)
                {
                    INodeSendPanel nodeUsePanel = GetComponent<INodeSendPanel>();
                    if (nodeUsePanel == null)
                    {
                        Debug.LogWarning("脚本未继承节点脚本接口");
                        return;
                    }
                    if (!string.IsNullOrEmpty(pair.nodePanel.name)) nodeUsePanel.OpenEvent += ((key,onclose, data) =>
                    {
                        if (pair.nodePanel.name != key) return;
                        if (pair.hideSelf)
                        {
                            if(pair.nodePanel.Open(() => { if (onclose != null) onclose(); UnHide(); }, data))
                            {
                                Hide();
                            }
                        }
                        else
                        {
                            pair.nodePanel.Open(onclose, data);
                        }

                    });
                }
            }
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public void UnHide()
        {
            gameObject.SetActive(true);
        }

        public bool Open(UnityAction onClose)
        {
            INodeReceivePanel panel = gameObject.GetComponent<INodeReceivePanel>();
            if ((panel != null && panel.CanOpen()) || panel == null)
            {
                UnHide();
                this.onClose = onClose;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Open(UnityAction onClose, object data)
        {
            INodeReceivePanel panel = gameObject.GetComponent<INodeReceivePanel>();
            if (panel != null && panel.CanOpen())
            {
                UnHide();
                panel.HandleOpenData(data);
                this.onClose = onClose;
                return true;
            }
            else if(panel == null)
            {
                UnHide();
                this.onClose = onClose;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            if ((openType & OpenType.ByName) == OpenType.ByName && !gameObject.GetComponent<INodeReceivePanel>().CloseAble()){
                return;
            }
            for (int i = 0; i < relatedPanels.Count; i++){
                relatedPanels[i].nodePanel.Hide();
            }
            if (onClose != null) onClose.Invoke();
            Hide();
        }


    }
}