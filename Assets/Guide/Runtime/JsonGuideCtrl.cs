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
            //TODO ��ָ��id��guide���
        }


        protected GuideInfoGroup LoadFromJson(string json)
        {
            var jsonMap = JsonUtility.FromJson<GuideJsonMap>(json);
            GuideInfoGroup infoGroup = new GuideInfoGroup();
            //TODO ��GuideJsonMap תΪGuideInfoGroup
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
        public string name;//˵��
        public List<KeyInfo> lunchFactor;//��������
        public List<GuideJsonStep> steps;//�����б�
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