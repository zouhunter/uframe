using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Guide
{
    public class JsonGuideCtrl : GuideCtrl
    {
        protected Dictionary<int, FactorCreateFunc> m_factorMap = new Dictionary<int, FactorCreateFunc>();
        protected GuideInfoGroup m_guideGroup;

        public void RegistFactor(int key, FactorCreateFunc makeFunc)
        {
            m_factorMap[key] = makeFunc;
        }

        public void ActiveGuides(string json, params int[] guideIds)
        {
            m_guideGroup = LoadFromJson(json);
            //TODO 将指定id的guide添加
        }


        protected GuideInfoGroup LoadFromJson(string json)
        {
            var jsonMap = JsonUtility.FromJson<GuideJsonMap>(json);
            GuideInfoGroup infoGroup = new GuideInfoGroup();
            //TODO 将GuideJsonMap 转为GuideInfoGroup
            return infoGroup;
        }

    }

    [System.Serializable]
    public class GuideJsonMap
    {
        public List<GuideJsonInfo> guides = new List<GuideJsonInfo>();
    }

    [System.Serializable]
    public class GuideJsonInfo
    {
        public int id;
        public string name;//说明
        public List<KeyInfo> lunchFactor;//触发条件
        public List<GuideJsonStep> steps;//步骤列表
    }

    [System.Serializable]
    public class GuideJsonStep
    {
        public int stepIndex;
        public string stepName;
        public KeyInfo action;
        public List<KeyInfo> startFactor;
        public List<KeyInfo> endFactor;
    }
}