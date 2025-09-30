////*************************************************************************************
////* 作    者： 邹杭特
////* 创建时间： 2021-10-16 10:12:43
////* 描    述：  

////* ************************************************************************************
//using System;
//using System.Collections.Generic;

//using UFrame.BgTransparent;

//using UnityEngine;


//public class MaterialChangeCtrl
//{
//    protected Dictionary<int, RendererOperation> m_renderMaps;

//    public string color_name = "_Color";
//    public string texture_name = "";
//    public string worp_color_name = "_BaseColor";
//    public string worp_texture_name = "";

//    public MaterialChangeCtrl()
//    {
//        m_renderMaps = new Dictionary<int, RendererOperation>();
//    }

//    public void ChangeMaterial(GameObject target, Material mat, bool needRecover)
//    {
//        if (target)
//        {
//            if (!m_renderMaps.TryGetValue(target.GetInstanceID(), out var renderOp))
//            {
//                var renders = target.GetComponent<Renderer>();
//                renderOp = new RendererOperation(mat, new Renderer[] { renders }, color_name, texture_name, worp_color_name, worp_texture_name);
//                if (needRecover)
//                {
//                    m_renderMaps[target.GetInstanceID()] = renderOp;
//                }
//            }
//            renderOp.WorpRenderers();
//        }
//    }

//    public void RecoverMaterial(GameObject target)
//    {
//        if (target)
//        {
//            if (m_renderMaps.TryGetValue(target.GetInstanceID(), out var renderOp))
//            {
//                renderOp.Recovery();
//            }
//        }
//    }
//}