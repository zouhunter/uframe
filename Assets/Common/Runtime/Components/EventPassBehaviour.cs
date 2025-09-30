/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - UI事件透传                                                                      *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/EventPassBehaviour")]
    public class EventPassBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event UnityAction onPointDown;
        public event UnityAction onPointUp;
        public event UnityAction onPointClick;
        public event UnityAction onPointEnter;
        public event UnityAction onPointExit;

        //监听按下
        public void OnPointerDown(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerDownHandler);
            if (onPointDown != null) onPointDown.Invoke();
        }

        //监听抬起
        public void OnPointerUp(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerUpHandler);
            if (onPointUp != null) onPointUp();
        }

        //监听点击
        public void OnPointerClick(PointerEventData eventData)
        {
            //PassEvent(eventData, ExecuteEvents.submitHandler);
            PassEvent(eventData, ExecuteEvents.pointerClickHandler);
            if (onPointClick != null) onPointClick();
        }
        //鼠标移入
        public void OnPointerEnter(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerEnterHandler);
            if (onPointEnter != null) onPointEnter();
        }
        //鼠标移出 
        public void OnPointerExit(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerExitHandler);
            if (onPointExit != null) onPointExit();
        }
        //把事件透下去
        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            GameObject current = data.pointerCurrentRaycast.gameObject;
            for (int i = 0; i < results.Count; i++)
            {
                if (current != results[i].gameObject)
                {
                    ExecuteEvents.Execute(results[i].gameObject, data, function);
                    //RaycastAll后ugui会自己排序，如果你只想响应透下去的最近的一个响应，这里ExecuteEvents.Execute后直接break就行。
                }
            }
        }
    }
}