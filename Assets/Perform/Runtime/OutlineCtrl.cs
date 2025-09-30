////*************************************************************************************
////* 作    者： z hunter
////* 创建时间： 2021-11-29 05:26:41
////*  描    述：

////* ************************************************************************************
//using System;
//using System.Collections.Generic;

//using UnityEngine;


//public class OutlineCtrl
//{
//    protected Dictionary<int, UFrame.Preform.OutlineBehaviour> m_outlineMap;

//    public OutlineCtrl()
//    {
//        m_outlineMap = new Dictionary<int, UFrame.Preform.OutlineBehaviour>();
//    }

//    public void ShowOutline(GameObject target, bool on, Color color, float width, UFrame.Preform.OutlineBehaviour.Mode outlineMode)
//    {
//        if (!target)
//            return;

//        var insanceId = target.GetInstanceID();
//        if ((!m_outlineMap.TryGetValue(insanceId, out var outline) || !outline) && on)
//        {
//            outline = target.GetComponent<UFrame.Preform.OutlineBehaviour>();
//            if (!outline)
//            {
//                outline = target.AddComponent<UFrame.Preform.OutlineBehaviour>();
//                outline.OutlineColor = color;
//                outline.OutlineWidth = width;
//                outline.OutlineMode = outlineMode;
//            }
//            m_outlineMap[insanceId] = outline;
//        }
//        if (on)
//        {
//            outline.enabled = true;
//        }
//        else if (outline)
//        {
//            outline.enabled = false;
//        }
//    }

//    public void ChangeColor(GameObject target, Color color)
//    {

//    }
//}
