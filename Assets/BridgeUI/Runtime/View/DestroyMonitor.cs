/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 监听删除事件                                                                    *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{
    public class DestroyMonitor : MonoBehaviour
    {
        public UnityAction onDestroy { get; internal set; }
        public UnityAction onEnable { get; internal set; }
        public UnityAction onDisable { get; internal set; }

        protected virtual void OnEnable()
        {
            onEnable?.Invoke();
        }

        protected virtual void OnDisable()
        {
            onDisable?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}