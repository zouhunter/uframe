/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 父类面板属性                                                                    *
*   - 用于editor下反射加载，以显示为可用的父级模板                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PanelParentAttribute : System.Attribute
    {
        public int sortIndex;
        public PanelParentAttribute(int sortIndex = 0)
        {
            this.sortIndex = sortIndex;
        }
    }

}
