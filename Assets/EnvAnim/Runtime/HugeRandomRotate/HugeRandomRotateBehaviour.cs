/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-07-06                                                                   *
*  功能:                                                                              *
*   - 大量的物体随机旋转运动                                                          *
*//************************************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UFrame
{
    public class HugeRandomRotateBehaviour : MonoBehaviour
    {
        [SerializeField]
        private ComputeShader m_computer;
        [SerializeField]
        private float speedMin = 1;
        [SerializeField]
        private float speedMax = 1;
        private ComputeBuffer dataBuff;
        private int rotateKernel;
        private int countOfGroup;
        private Transform[] m_components;
        public struct BuffData
        {
            public float rotateSpeed;
            public Vector3 rotateAngle;
            public Vector3 eularAngle;
        }

        private BuffData[] datas;

        private void Start()
        {
            m_components = new Transform[transform.childCount];
            for (int i = 0; i < m_components.Length; i++) {
                m_components[i] = transform.GetChild(i);
            }
            countOfGroup = Mathf.CeilToInt(m_components.Length / 32);
            datas = new BuffData[m_components.Length];
            int genSpeedKernel = m_computer.FindKernel("RandomRotateAngle");
            var count = m_components.Length;
            dataBuff = new ComputeBuffer(count, sizeof(float) * 7);
            m_computer.SetBuffer(genSpeedKernel, "datas", dataBuff);
            m_computer.SetFloat("speedMin", speedMin);
            m_computer.SetFloat("speedMax", speedMax);
            m_computer.Dispatch(genSpeedKernel, countOfGroup, 1, 1);
            dataBuff.GetData(datas);

            rotateKernel = m_computer.FindKernel("RotateTo");
            m_computer.SetBuffer(rotateKernel, "datas", dataBuff);
        }

        private void Update()
        {
            m_computer.SetFloat("deltTime", Time.deltaTime);
            m_computer.Dispatch(rotateKernel, countOfGroup, 1, 1);
            dataBuff.GetData(datas);
            for (int i = 0; i < datas.Length; i++)
            {
                m_components[i].transform.eulerAngles = datas[i].eularAngle;
            }
        }
    }
}