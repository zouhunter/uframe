/*************************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2021-04-18                                                                   *
*  �汾: master_482d3f                                                                *
*  ����:                                                                              *
*   - �����ص�                                                                        *
*                                                                                     *
***************************************************************************************/

namespace UFrame.Guide
{
    public delegate void GuideActivedEvent(int guideId);
    public delegate void GuideStartEvent(int guideId, int stepId);
    public delegate void GuideStopEvent(int guideId, int stepId);
    public delegate void GuideFinishedEvent(int guideId, string error);
    public delegate bool GuideTriggerAction(string [] args);
    public delegate bool GuideStateCheckAction(string[] args);
}
