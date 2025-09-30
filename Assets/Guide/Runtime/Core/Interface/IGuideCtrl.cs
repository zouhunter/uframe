/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 引导控制器                                                                      *
*    -  执行引导步骤                                                                  *
*    -  判断引导状态                                                                  *
*    -  确认引导结束        .                                                         *
*                                                                                     *
***************************************************************************************/

namespace UFrame.Guide
{
    public interface IGuideCtrl
    {
        void AddGuidesSaftySort(params GuideInfo[] guideInfos);
        void RefreshGuide();
        bool StartGuiding(int guideId);
        bool StopGuiding();
        void RemoveGuide(int guideId);
        GuideStep GetCurrentGuideStep();
        void UndoCurrentStep();
        void SkipCurrentStep();
    }
}

