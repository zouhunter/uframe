/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 指定状态显示                                                                    *
*//************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/StateShowBehaviour")]
    public class StateShowBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected int showState = 0;
        [SerializeField]
        public List<GameObject> m_items;
        public void SetViewState(int state)
        {
            for (int i = 0; i < m_items.Count; i++)
            {
                m_items[i].SetActive(state == showState);
            }
        }
    }
}