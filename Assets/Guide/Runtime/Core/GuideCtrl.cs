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

using System;
using UnityEngine;
using System.Collections.Generic;


namespace UFrame.Guide
{
    public class GuideCtrl : IGuideCtrl
    {
        public event GuideActivedEvent guideActiveEvent;
        public event GuideStartEvent guideStartEvent;
        public event GuideStopEvent guideStopEvent;
        public event GuideFinishedEvent guideFinishEvent;
        protected Dictionary<int, GuideInfo> m_guideDic = new Dictionary<int, GuideInfo>();
        protected GuideState m_state = new GuideState();
        protected bool m_allGuideActived = false;

        public void ClearAll()
        {
            m_guideDic.Clear();
            m_state = new GuideState();
            m_allGuideActived = false;
            guideActiveEvent = null;
            guideStartEvent = null;
            guideStopEvent = null;
            guideFinishEvent = null;
        }

        public bool AddGuideSafty(GuideInfo guideInfo, bool sort)
        {
            if (guideInfo != null)
            {
                if (guideInfo.steps.Count <= 0)
                {
                    Debug.LogError("guide empty steps!");
                    return false;
                }
                for (int j = 0; j < guideInfo.steps.Count; j++)
                {
                    var step = guideInfo.steps[j];
                    if (step == null)
                    {
                        Debug.LogError("guide step empty!");
                        return false;
                    }
                }
                if (sort)
                {
                    guideInfo.steps.Sort((x, y) => x.stepId - y.stepId);
                }
                m_guideDic[guideInfo.id] = guideInfo;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddGuidesSaftySort(params GuideInfo[] guides)
        {
            for (int i = 0; i < guides.Length; i++)
            {
                var guideInfo = guides[i];
                if (guideInfo != null)
                {
                    var ok = AddGuideSafty(guideInfo, true);
                    if (!ok)
                        Debug.LogError("guide add failed at:" + i);
                }
                else
                    Debug.LogError("guide empty at:" + i);
            }
        }

        public void RefreshGuide()
        {
            if (m_state.guidingTask != 0)
            {
                UpdateActiveTask();
            }
            else if (!m_allGuideActived)
            {
                FindOneGuideAbles();
            }
        }

        private void UpdateActiveTask()
        {
            if (m_guideDic.TryGetValue(m_state.guidingTask, out GuideInfo guideInfo) && guideInfo.steps != null)
            {
                if (guideInfo.steps.Count > m_state.guidingStepIndex)
                {
                    if (m_state.guidingStartedStepIndex == m_state.guidingStepIndex && CheckFactors(guideInfo.steps[m_state.guidingStepIndex].endFactor))
                    {
                        OnStopGuide(m_state.guidingTask, m_state.guidingStepIndex);
                        m_state.guidingStepIndex++;
                    }

                    if (m_state.guidingStepIndex < guideInfo.steps.Count && m_state.guidingStartedStepIndex != m_state.guidingStepIndex)
                    {
                        bool right = TryStartGuide(guideInfo, m_state.guidingStepIndex);
                        if (right)
                        {
                            m_state.guidingStartedStepIndex = m_state.guidingStepIndex;
                        }
                    }
                }
                else
                {
                    OnGuideFinish(m_state.guidingTask);
                }
            }
            else
            {
                OnGuideFinish(m_state.guidingTask, "data lost!");
            }
        }

        private void FindOneGuideAbles()
        {
            var allActived = true;
            foreach (var pair in m_guideDic)
            {
                var id = pair.Key;
                if (m_state.activeTasks.Contains(id))
                    continue;

                allActived = false;
                if (CheckFactors(pair.Value.lunchFactor))
                {
                    m_state.activeTasks.Add(id);
                    OnGuideActived(id);
                    break;
                }
            }
            m_allGuideActived = allActived;
        }

        protected virtual void OnGuideActived(int guideId)
        {
            if (guideActiveEvent != null)
            {
                guideActiveEvent.Invoke(guideId);
            }
        }

        private bool StartGuidingInternal(int guideId)
        {
            if (!m_guideDic.TryGetValue(guideId, out GuideInfo guideInfo))
            {
                Debug.LogError("guide task not exists:" + guideId);
                return false;
            }
            else if (!m_state.activeTasks.Contains(guideId))
            {
                Debug.LogError("guiding not active:" + guideId);
                return false;
            }

            if (m_state.guidingTask != 0 && m_state.guidingTask != guideId)
            {
                Debug.LogError("guiding changed but other not completed!" + m_state.guidingTask);
                return false;
            }

            if (m_state.guidingTask == guideId)
            {
                if (m_state.guidingStepIndex < guideInfo.steps.Count && m_state.guidingStepIndex >= 0)
                {
                    return true;
                }
            }

            if (guideInfo.steps == null || guideInfo.steps.Count <= 0)
            {
                Debug.LogError("guiding finish because step_num==0!");
                return false;
            }
            m_state.guidingTask = guideId;
            m_state.guidingStepIndex = 0;
            m_state.guidingStartedStepIndex = -1;
            return true;
        }

        public virtual bool StartGuiding(int guideId)
        {
            return StartGuidingInternal(guideId);
        }

        public bool CheckFactors(IEnumerable<IFactor> factors)
        {
            if (factors == null)
                return true;

            bool all = true;
            foreach (var factor in factors)
            {
                if (factor == null)
                    continue;

                all &= factor.Process();
                if (all == false)
                    break;
            }
            return all;
        }

        public bool StopGuiding()
        {
            if (m_state.guidingTask != 0)
            {
                OnStopGuide(m_state.guidingTask, m_state.guidingStepIndex);
                m_state.ResetGuideSteps();
                return true;
            }
            return false;
        }

        protected virtual void OnStopGuide(int guideId, int stepId)
        {
            if (guideStopEvent != null)
            {
                guideStopEvent.Invoke(guideId, stepId);
            }
        }

        protected bool TryStartGuide(GuideInfo guideInfo, int stepId)
        {
            var stepInfo = guideInfo.steps[stepId];
            if (!CheckFactors(stepInfo.startFactor))
            {
                //Debug.Log("TryStartGuide waiting...step:" + stepId);
                return false;
            }

            OnStartGuide(guideInfo.id, stepId);
            return true;
        }

        protected virtual void OnStartGuide(int guideId, int stepId)
        {
            m_state.guidingTask = guideId;
            m_state.guidingStepIndex = stepId;

            if (guideStartEvent != null)
            {
                guideStartEvent.Invoke(guideId, stepId);
            }
        }

        protected virtual void OnGuideFinish(int guideId, string err = null)
        {
            m_state.ResetGuideSteps();

            if (guideFinishEvent != null)
            {
                guideFinishEvent.Invoke(guideId, err);
            }
        }

        public void RegistOnGuideActive(GuideActivedEvent callback, bool regist = true)
        {
            this.guideActiveEvent -= callback;
            if (regist)
                this.guideActiveEvent += callback;
        }

        public void RegistOnGuideStart(GuideStartEvent callback, bool regist = true)
        {
            this.guideStartEvent -= callback;
            if (regist)
                this.guideStartEvent += callback;
        }

        public void RegistOnGuideStop(GuideStopEvent callback, bool regist = true)
        {
            this.guideStopEvent -= callback;
            if (regist)
                this.guideStopEvent += callback;
        }

        public void RegistOnGuideFinish(GuideFinishedEvent callback, bool regist = true)
        {
            this.guideFinishEvent -= callback;
            if (regist)
                this.guideFinishEvent += callback;
        }

        public GuideStep GetCurrentGuideStep()
        {
            if (m_state != null && m_guideDic.TryGetValue(m_state.guidingTask, out var guideInfo))
            {
                if (m_state.guidingStepIndex >= 0 && m_state.guidingStepIndex < guideInfo.steps.Count)
                    return guideInfo.steps[m_state.guidingStepIndex];
            }
            return null;
        }

        private void UndoStepInternal(GuideStep step)
        {
            for (int i = step.endFactor.Count - 1; i >= 0; i--)
            {
                var factor = step.endFactor[i];
                if (factor is IUndoAble)
                {
                    (factor as IUndoAble).Undo();
                }
            }
            for (int i = step.startFactor.Count - 1; i >= 0; i--)
            {
                var factor = step.startFactor[i];
                if (factor is IUndoAble)
                {
                    (factor as IUndoAble).Undo();
                }
            }
            m_state.guidingStartedStepIndex = m_state.guidingStepIndex - 1;
        }

        public void UndoCurrentStep()
        {
            var currentStep = GetCurrentGuideStep();
            if (currentStep != null)
            {
                UndoStepInternal(currentStep);
            }
            else
            {
                Debug.LogError("current step empty!");
            }
        }

        private void SkipCurrentStepInternal(GuideStep step)
        {
            if (m_state.guidingStartedStepIndex != m_state.guidingStepIndex)
            {
                foreach (var factor in step.startFactor)
                {
                    if (factor is ISkipAble)
                    {
                        (factor as ISkipAble).Skip();
                    }
                }
            }

            foreach (var factor in step.endFactor)
            {
                if (factor is ISkipAble)
                {
                    (factor as ISkipAble).Skip();
                }
            }
        }

        public void SkipCurrentStep()
        {
            var currentStep = GetCurrentGuideStep();
            if (currentStep != null)
            {
                SkipCurrentStepInternal(currentStep);
            }
            else
            {
                Debug.LogError("current step empty!");
            }
        }

        public void RemoveGuide(int guideId)
        {
            if (m_state.guidingTask == guideId)
            {
                m_state.guidingTask = 0;
                m_state.guidingStepIndex = 0;
                m_state.guidingStartedStepIndex = -1;
            }
            m_guideDic.Remove(guideId);
        }
    }
}

