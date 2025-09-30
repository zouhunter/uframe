/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 引导步骤模板                                                                    *
*                                                                                     *
***************************************************************************************/
using System.Collections;
using System.Collections.Generic;

namespace UFrame.Guide
{
    [System.Serializable]
    public class GuideStep
    {
        public int stepId;
        public string stepName;
        public List<IFactor> startFactor;
        public List<IFactor> endFactor;
        public GuideStep() { }
        public GuideStep(int stepId,string stepName)
        {
            this.stepId = stepId;
            this.stepName = stepName;
        }

        public void AddStartFactor(IFactor factor)
        {
            if (startFactor == null)
                startFactor = new List<IFactor>();
            startFactor.Add(factor);
        }

        public void AddEndFactor(IFactor factor)
        {
            if (endFactor == null)
                endFactor = new List<IFactor>();
            endFactor.Add(factor);
        }
    }
}