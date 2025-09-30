//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-02-10 10:44:34
//* 描    述： 基于表格的ui引用

//* ************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class UIPanelGroup : PanelGroupBase
    {
        [SerializeField]
        public List<TextAsset> panelConfigs;
        [SerializeField]
        public List<TextAsset> connectionConfigs;
        [SerializeField]
        public List<BundleUIInfo> b_nodes = new List<BundleUIInfo>();
        [SerializeField]
        public List<BridgeInfo> bridges = new List<BridgeInfo>();

        private Dictionary<string, UIInfoBase> m_nodes;
        public override List<BridgeInfo> Bridges => bridges;
        public override Dictionary<string, UIInfoBase> Nodes
        {
            get
            {
                if (m_nodes == null)
                {
                    m_nodes = new Dictionary<string, UIInfoBase>();
                    if (b_nodes != null)
                        b_nodes.ForEach(x =>
                        {
                            m_nodes.Add(x.panelName, x);
                        });
                }
                return m_nodes;
            }
        }

        public bool UseAssetBundle => bundleCreateRule != null;

        private void Awake()
        {
            LunchPanelGroupSystem();
        }
    }
}