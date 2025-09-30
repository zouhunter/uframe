//*************************************************************************************
//* 作    者： z hunter
//* 创建时间： 2021-11-29 05:28:40
//*  描    述：

//* ************************************************************************************
using System;
using UnityEngine;

public class FlashOnData
{
    public GameObject target;
    public Color startColor;
    public Color endColor;
    public float ratio;

    public FlashOnData(GameObject target, Color startColor, Color endColor)
    {
        this.target = target;
        this.startColor = startColor;
        this.endColor = endColor;
        ratio = 0;
    }
}