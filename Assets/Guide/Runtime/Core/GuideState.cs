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
using System.Collections;
using System;
using System.Collections.Generic;

namespace UFrame.Guide
{
    public class GuideState
    {
        public bool valid = false;
        public HashSet<int> activeTasks = new HashSet<int>();
        public int guidingTask = 0;
        public int guidingStepIndex = 0;
        public int guidingStartedStepIndex = 0;

        public void ResetGuideSteps()
        {
            guidingTask = 0;
            guidingStepIndex = 0;
            guidingStartedStepIndex = -1;
        }
    }
}
