/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 点数据                                                                          *
*//************************************************************************************/

namespace UFrame.NodeGraph.DataModel
{
    [System.Serializable]
    public struct Point
    {
        public string label;
        public string type;
        public int max;
        public Point(string label, string type, int max = 1)
        {
            this.label = label;
            this.type = type;
            this.max = max;
        }
    }
}