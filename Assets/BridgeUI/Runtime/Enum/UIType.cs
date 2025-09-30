/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 显示规则相关                                                                    *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public struct UIType
    {
        //层级优先
        public int layerIndex;
        //位置控制
        public UIFormType form;
        //绝对层级
        public UILayerType layer;
        //隐藏透明度
        public float hideAlaph;
        //关闭规则
        public CloseRule closeRule;
        //隐藏规则
        public HideRule hideRule;
        //动态遮罩
        public UIMask cover;
        //遮罩颜色
        public UnityEngine.Color maskColor;

        public UIType(UnityEngine.Color maskColor)
        {
            this.layerIndex = 0;
            this.form = UIFormType.Fixed;
            this.hideAlaph = 0;
            this.layer = UILayerType.Base;
            this.closeRule = CloseRule.DestroyNoraml;
            this.hideRule = HideRule.HideGameObject;
            this.cover = UIMask.None;
            this.maskColor = maskColor;
        }
    }
}