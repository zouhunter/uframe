/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - UI连接器池                                                                      *
*//************************************************************************************/

using UFrame.BridgeUI;

namespace UFrame.BridgeUI
{
    public class BridgePool
    {
        private ObjectPool<Bridge> innerPool;
        public BridgePool()
        {
            innerPool = new ObjectPool<Bridge>(CreateInstence);
        }

        public Bridge CreateInstence()
        {
            var bridge = new Bridge(OnRelease);
            return bridge;
        }

        internal Bridge Allocate(BridgeInfo info,IUIPanel parentPanel = null)
        {
            var bridge = innerPool.Allocate();
            bridge.ResetInfo(info);
            bridge.SetInPanel(parentPanel);
            return bridge;
        }
        private void OnRelease(Bridge bridge)
        {
            innerPool.Release(bridge);
        }
    }
}