/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 遮罩设置                                                                        *
*                                                                                     *
***************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace UFrame
{
    [RequireComponent(typeof(Image))]
    public class GuideMaskBehaivour : MonoBehaviour
    {
        [SerializeField]
        protected EventPassBehaviour m_eventPass;
        [SerializeField]
        protected string m_radiusKey = "Radius";
        [SerializeField]
        protected string m_centerKey = "Center";
        protected Material m_material;

        private void Awake()
        {
            m_material = GetComponent<Image>().material;
        }

        public virtual void SetMask(Rect rect)
        {

        }
    }
}