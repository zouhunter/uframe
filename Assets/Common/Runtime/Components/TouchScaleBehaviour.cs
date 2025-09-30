/*************************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2021-08-22                                                                   *
*  ����:                                                                              *
*   - �ƶ��������ж�                                                                  *
*//************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/TouchScaleBehaviour")]
    public class TouchScaleBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected UnityEvent<float> onScaleOffset;
        private float m_touchDistance;
        private void Update()
        {
            if (Input.touchCount >= 2)
            {
                Touch newTouch1 = Input.GetTouch(0);
                Touch newTouch2 = Input.GetTouch(1);
                var distance = Vector2.Distance(newTouch1.position, newTouch2.position);
                if (m_touchDistance > 1 && onScaleOffset != null)
                {
                    float offset = distance - m_touchDistance;
                    onScaleOffset.Invoke(offset);
                }
                m_touchDistance = distance;
            }
        }
    }
}