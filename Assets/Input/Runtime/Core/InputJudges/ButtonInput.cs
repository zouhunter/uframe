/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 按扭输入判断                                                                    *
*//************************************************************************************/

namespace UFrame.Inputs
{
    public class ButtonInputJudge : StringInputJudge
    {
        private IVirtualValueGetter inputCtrl;
        public ButtonInputJudge(IVirtualValueGetter inputCtrl)
        {
            this.inputCtrl = inputCtrl;
        }
        protected override bool IsTriggered(string key)
        {
            if (!inputCtrl.ButtonExists(key)) return false;

            return inputCtrl.GetButton(key);
        }
    }
    public class ButtonUpInputJudge : StringInputJudge
    {
        private IVirtualValueGetter inputCtrl;
        public ButtonUpInputJudge(IVirtualValueGetter inputCtrl)
        {
            this.inputCtrl = inputCtrl;
        }
        protected override bool IsTriggered(string key)
        {
            if (!inputCtrl.ButtonExists(key)) return false;

            return inputCtrl.GetButtonUp(key);
        }
    }
    public class ButtonDownInputJudge : StringInputJudge
    {
        private IVirtualValueGetter inputCtrl;
        public ButtonDownInputJudge(IVirtualValueGetter inputCtrl)
        {
            this.inputCtrl = inputCtrl;
        }
        protected override bool IsTriggered(string key)
        {
            if (!inputCtrl.ButtonExists(key)) return false;

            return inputCtrl.GetButtonDown(key);
        }
    }
}
