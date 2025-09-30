/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 引导任务                                                                        *
*                                                                                     *
***************************************************************************************/

using System.Collections.Generic;

namespace UFrame.Guide
{
    [System.Serializable]
    public class GuideInfo
    {
        public int id;
        public string name;
        public List<IFactor> lunchFactor;
        public List<GuideStep> steps;
    }
}
