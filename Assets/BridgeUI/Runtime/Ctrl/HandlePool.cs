/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - UI句柄池                                                                        *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    public class UIHandlePool
    {
        private ObjectPool<UIHandle> innerPool;

        public UIHandlePool()
        {
            innerPool = new ObjectPool<UIHandle>(CreateInstence);
        }

        public UIHandle Allocate(string panelName)
        {
            UIHandle uiHandle = innerPool.Allocate();
            uiHandle.Reset(panelName, OnRelease);
            return uiHandle;
        }

        private UIHandle CreateInstence()
        {
            return new UIHandle();
        }

        private void OnRelease(UIHandle handle)
        {
            innerPool.Release(handle);
        }
    }
}