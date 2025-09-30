/*************************************************************************************//*
*  ����: �޺���                                                                       *
*  ʱ��: 2021-06-27                                                                   *
*  ����:                                                                              *
*   - ѡ������ʾ                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

using UnityEngine;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/ToggleShowHideBehaviour")]
    public class ToggleShowHideBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected int showState = 0;
        [SerializeField]
        public List<GameObject> m_itemShows;
        [SerializeField]
        public List<GameObject> m_itemHides;

        public void SetViewState(int state)
        {
            for (int i = 0; i < m_itemShows.Count; i++)
            {
                m_itemShows[i].SetActive(state == showState);
            }
            for (int i = 0; i < m_itemHides.Count; i++)
            {
                m_itemHides[i].SetActive(state != showState);
            }
        }
    }
}