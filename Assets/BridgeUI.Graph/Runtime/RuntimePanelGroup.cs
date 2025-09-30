/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面组-内置                                                                     *
*//************************************************************************************/

using UFrame.BridgeUI;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class RuntimePanelGroup : PanelGroupBase
    {
        public HashSet<Graph.UIGraph> m_uiDatas = new HashSet<Graph.UIGraph>();
        protected List<BridgeInfo> m_bridges = new List<BridgeInfo>();
        protected Dictionary<string, UIInfoBase> m_nodes = new Dictionary<string, UIInfoBase>();
        public override List<BridgeInfo> Bridges => m_bridges;
        public override Dictionary<string, UIInfoBase> Nodes => m_nodes;

        public void RegistUIData(Graph.UIGraph uiData)
        {
            if (!m_uiDatas.Contains(uiData))
            {
                m_uiDatas.Add(uiData);
                OnRegistUIData(uiData);
            }
        }

        protected virtual void OnRegistUIData(Graph.UIGraph uiData)
        {
            for (int i = 0; i < uiData.bridges.Count; i++)
            {
                var bridgeItem = uiData.bridges[i];
                var oldIndex = Bridges.FindIndex(x => x.inNode == bridgeItem.inNode && x.outNode == bridgeItem.outNode);
                if (oldIndex >= 0)
                {
                    Bridges[oldIndex] = bridgeItem;
                }
            }
            switch (uiData.loadType)
            {
                case LoadType.DirectLink:
                    foreach (var item in uiData.p_nodes)
                    {
                        m_nodes[item.panelName] = item;
                    }
                    break;
                case LoadType.Resources:
                    foreach (var item in uiData.r_nodes)
                    {
                        m_nodes[item.panelName] = item;
                    }
                    break;
                case LoadType.Bundle:
                    foreach (var item in uiData.b_nodes)
                    {
                        m_nodes[item.panelName] = item;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}