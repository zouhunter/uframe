/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 轴输入句柄                                                                      *
*//************************************************************************************/

namespace UFrame.Inputs
{
    public class AxisInputHandle : StringInputFloatValueHandle
    {
        private IVirtualValueGetter inputCtrl;
        public AxisInputHandle(IVirtualValueGetter inputCtrl)
        {
            this.inputCtrl = inputCtrl;
        }
        protected override float GetValue(string key)
        {
            return inputCtrl.GetAxis(key);
        }
    }

    public class AxisRawInputHandle : StringInputFloatValueHandle
    {
        private IVirtualValueGetter inputCtrl;
        public AxisRawInputHandle(IVirtualValueGetter inputCtrl)
        {
            this.inputCtrl = inputCtrl;
        }
        protected override float GetValue(string key)
        {
            if (!inputCtrl.AxisExists(key)) return 0;
            return inputCtrl.GetAxisRaw(key);
        }
    }

}