using System;
using System.Collections.Generic;
using UnityEngine;
using UFrame.TableCfg;

namespace UFrame.Guide
{
    public class TableGuideCtrl : GuideCtrl
    {
        protected Dictionary<int, FactorCreateFunc> m_factorMap = new Dictionary<int, FactorCreateFunc>();
        protected Table<int,GuideCfg> m_guideCfgTable;
        protected Table<int,GuideStepCfg> m_stepCfgTable;
        protected Table<int,GuideFactorCfg> m_factorCfgTable;

        public void RegistFactor(int key, FactorCreateFunc makeFunc)
        {
            m_factorMap[key] = makeFunc;
        }

        public void RegistConfigs(Table<int,GuideCfg> guideCfg, Table<int,GuideStepCfg> stepCfg, Table<int, GuideFactorCfg> factorCfg)
        {
            this.m_guideCfgTable = guideCfg;
            this.m_stepCfgTable = stepCfg;
            this.m_factorCfgTable = factorCfg;
        }

        public void ActiveGuides(params int[] guideIds)
        {
            if (m_guideCfgTable == null || m_stepCfgTable == null || m_factorCfgTable == null)
            {
                Debug.LogError("guide config empty!");
                return;
            }

            for (int i = 0; i < guideIds.Length; i++)
            {
                var guideId = guideIds[i];
                var guidCfg = m_guideCfgTable.GetByKey(guideId);
                if (guidCfg != null)
                {
                    GuideInfo info = new GuideInfo();
                    info.name = guidCfg.name;
                    info.id = guideId;
                    if (guidCfg.lunch_factors != null)
                    {
                        info.lunchFactor = new List<IFactor>();
                        for (int j = 0; j < guidCfg.lunch_factors.Length; j++)
                        {
                            var factorId = guidCfg.lunch_factors[i];
                            var factor = GetFactorFromCfg(factorId);
                            if (factor != null)
                            {
                                info.lunchFactor.Add(factor);
                            }
                        }
                    }

                    if (guidCfg.steps != null)
                    {
                        info.steps = new List<GuideStep>();
                        for (int j = 0; j < guidCfg.steps.Length; j++)
                        {
                            var stepId = guidCfg.steps[j];
                            GuideStep guideStep = GetGuideStepFromCfg(stepId);
                            if (guideStep != null)
                            {
                                info.steps.Add(guideStep);
                            }
                        }
                    }
                    AddGuidesSaftySort(info);
                }
            }
        }

        protected List<IFactor> GetFactorsFromCfg(int[] factorIds)
        {
            if (factorIds != null)
            {
                var factors = new List<IFactor>();
                for (int i = 0; i < factorIds.Length; i++)
                {
                    var factor = GetFactorFromCfg(factorIds[i]);
                    if(factor != null)
                    {
                        factors.Add(factor);
                    }
                }
                return factors;
            }
            return null;
        }

        protected IFactor GetFactorFromCfg(int factorId)
        {
            var factorCfg = m_factorCfgTable?.GetByKey(factorId);
            if (factorCfg != null && m_factorMap.TryGetValue(factorCfg.type_id, out var makeFunc) && makeFunc != null)
            {
                return makeFunc.Invoke(factorCfg.args);
            }
            else
            {
                Debug.LogError("factor not reg:" + factorId);
                return null;
            }
        }

        protected GuideStep GetGuideStepFromCfg(int stepId)
        {
            var stepCfg = m_stepCfgTable.GetByKey(stepId);
            if (stepCfg == null)
                return null;
            GuideStep step = new GuideStep();
            step.stepName = stepCfg.step_name;
            step.stepId = stepCfg.step_index;
            step.startFactor = GetFactorsFromCfg(stepCfg.start_factors);
            step.endFactor = GetFactorsFromCfg(stepCfg.end_factors);
            return step;
        }
    }
}