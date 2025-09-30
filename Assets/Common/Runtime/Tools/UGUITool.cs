/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   UGUI相关工具。                                                                *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UFrame
{
    public static class UGUITool
    {
        static PointerEventData eventDatas = new PointerEventData(EventSystem.current);
        static List<RaycastResult> hit = new List<RaycastResult>();

        public static bool IsHoverUI(Vector2 postion, int uiLayer = 5)
        {
            eventDatas.position = postion;
            eventDatas.pressPosition = postion;
            EventSystem.current.RaycastAll(eventDatas, hit);

            if (hit.Count > 0)
            {
                for (int i = 0; i < hit.Count; i++)
                {
                    if (hit[i].gameObject.layer == uiLayer)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
		 #region Rect扩展
        public static Rect Move(this Rect rect, float x, float y)
        {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }
        public static Rect MoveX(this Rect rect, float x)
        {
            return new Rect(rect.x + x, rect.y, rect.width, rect.height);
        }
        public static Rect MoveY(this Rect rect, float y)
        {
            return new Rect(rect.x, rect.y + y, rect.width, rect.height);
        }
        public static Rect Padding(this Rect rect, float paddingX, float paddingY)
        {
            return new Rect(rect.x + paddingX, rect.y + paddingY, rect.width - paddingX * 2, rect.height - paddingY * 2);
        }
        public static Rect Border(this Rect rect, float borderX, float borderY)
        {
            return new Rect(rect.x - borderX, rect.y - borderY, rect.width + borderX * 2, rect.height + borderY * 2);
        }
        public static Rect ReSize(this Rect rect, float width, float height)
        {
            return new Rect(rect.x, rect.y, width, height);
        }
        public static Rect ReSizeW(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, width, rect.height);
        }
        public static Rect ReSizeH(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, height);
        }
        #endregion
    }
}