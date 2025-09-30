/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 创建信息池                                                                      *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    public class UICreateInfoPool
    {
        private ObjectPool<UICreateInfo> innerPool;
        public UICreateInfoPool()
        {
            innerPool = new ObjectPool<UICreateInfo>(CreateInstence);
        }

        public UICreateInfo CreateInstence()
        {
            var bridge = new UICreateInfo();
            return bridge;
        }

        internal UICreateInfo Allocate(UIInfoBase info, UnityAction<GameObject> onCreate)
        {
            var uicreateInfo = innerPool.Allocate();
            uicreateInfo.uiInfo = info;
            uicreateInfo.onCreate = onCreate;
            return uicreateInfo;
        }
        public void Release(UICreateInfo uiCreateInfo)
        {
            innerPool.Release(uiCreateInfo);
        }
    }
    public class UICreateInfo
    {
        public UIInfoBase uiInfo;
        public UnityAction<GameObject> onCreate;
    }
}