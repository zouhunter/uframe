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
    /// <summary>
    /// 用于标记ui打开的父级
    /// [3维场景中可能有多个地方需要打开用户界面]
    /// </summary>
    public class PanelGroup : PanelGroupBase
    {
        [UnityEngine.SerializeField]
        protected List<string> graphList;
        [UnityEngine.SerializeField]
        protected List<BridgeInfo> bridges;
        [UnityEngine.SerializeField]
        protected List<PrefabUIInfo> p_nodes;
        [UnityEngine.SerializeField]
        protected List<ResourceUIInfo> r_nodes;
        [UnityEngine.SerializeField]
        protected List<BundleUIInfo> b_nodes;


        private Dictionary<string, UIInfoBase> infoDic;

        public override List<BridgeInfo> Bridges
        {
            get
            {
                return bridges;
            }
        }
        public override Dictionary<string, UIInfoBase> Nodes
        {
            get
            {
                if (infoDic == null)
                {
                    infoDic = new Dictionary<string, UIInfoBase>();
                    if (b_nodes != null)
                        b_nodes.ForEach(x =>
                        {
                            infoDic.Add(x.panelName, x);
                        });
                    if (r_nodes != null)
                        r_nodes.ForEach(x =>
                        {
                            infoDic.Add(x.panelName, x);
                        });
                    if (p_nodes != null)
                        p_nodes.ForEach(x =>
                        {
                            infoDic.Add(x.panelName, x);
                        });
                }
                return infoDic;
            }
        }
        public List<PrefabUIInfo> P_nodes
        {
            get
            {
                if (p_nodes == null)
                {
                    p_nodes = new List<PrefabUIInfo>();
                }
                return p_nodes;
            }
        }
        public List<BundleUIInfo> B_nodes
        {
            get
            {
                if (b_nodes == null)
                {
                    b_nodes = new List<BundleUIInfo>();
                }
                return b_nodes;
            }
        }
        public List<ResourceUIInfo> R_nodes
        {
            get
            {
                if (r_nodes == null)
                {
                    r_nodes = new List<ResourceUIInfo>();
                }
                return r_nodes;
            }
        }
        public List<string> GraphList
        {
            get
            {
                if (graphList == null)
                {
                    graphList = new List<string>();
                }
                return graphList;
            }
        }

        protected virtual void Start()
        {
            LunchPanelGroupSystem();
        }

        public string GetGraphAtIndex(int id)
        {
            if (graphList != null && graphList.Count > id && id >= 0)
            {
                return graphList[id];
            }
            return null;
        }
    }
}