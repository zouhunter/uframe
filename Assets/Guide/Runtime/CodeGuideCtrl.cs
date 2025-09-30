using System.Collections.Generic;

namespace UFrame.Guide
{
    public class CodeGuideCtrl : GuideCtrl
    {
        public bool CreateGuide(int guidId, IEnumerable<IFactor> factors)
        {
            return CreateGuideInternal(guidId, "", factors,null);
        }
        public bool CreateGuide(int guidId, string name, IEnumerable<IFactor> factors)
        {
            return CreateGuideInternal(guidId, name, factors, null);
        }
        public bool CreateGuide(int guidId, string name, IEnumerable<IFactor> factors, IEnumerable<GuideStep> steps)
        {
            return CreateGuideInternal(guidId, name, factors, steps);
        }
        private bool CreateGuideInternal(int guidId, string name, IEnumerable<IFactor> factors, IEnumerable<GuideStep> steps)
        {
            if (!m_guideDic.TryGetValue(guidId, out var guide))
            {
                guide = new GuideInfo();
                guide.id = guidId;
                guide.name = name;
            }
            if (guide.lunchFactor == null)
                guide.lunchFactor = new List<IFactor>();
            else
                guide.lunchFactor.Clear();
            if (guide.steps == null)
                guide.steps = new List<GuideStep>();
            else
                guide.steps.Clear();
            if (factors != null)
                guide.lunchFactor.AddRange(factors);
            if (steps != null)
            {
                guide.steps.AddRange(steps);
                m_guideDic[guidId] = guide;
                return true;
            }
            else
            {
                m_guideDic[guidId] = guide;
                return true;
            }
        }
    }
}