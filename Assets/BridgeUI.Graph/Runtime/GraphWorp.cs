/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 图表转换                                                                        *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class GraphWorp
    {
        public string graphName;
        public string guid;

        public GraphWorp(string graphName, string guid)
        {
            this.graphName = graphName;
            this.guid = guid;
        }
    }
}