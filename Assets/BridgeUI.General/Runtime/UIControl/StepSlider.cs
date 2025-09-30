//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2022-01-21 17:27:38
//* 描    述： 单点滑动控件

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UFrame.BridgeUI;

public class StepSlider : BridgeUIControl
{
    private List<Toggle> m_toggleList;
    private float m_value;
    public float Value { get { return m_value; } set { SetValue(value); } }

    protected override void OnInitialize()
    {
        if (m_toggleList == null)
            m_toggleList = new List<Toggle>();
        m_toggleList.AddRange(GetComponentsInChildren<Toggle>());
        foreach (var toggle in m_toggleList)
        {
            toggle.isOn = false;
        }
    }

    protected override void OnRelease()
    {
        m_toggleList.Clear();
    }

    protected void SetValue(float value)
    {
        if (m_toggleList != null)
        {
            var progress = Mathf.CeilToInt(value * m_toggleList.Count);
            for (int i = 0; i < m_toggleList.Count; i++)
            {
                m_toggleList[i].isOn = i < progress;
            }
        }
    }
}
