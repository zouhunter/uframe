////*************************************************************************************
////* 作    者： z hunter
////* 创建时间： 2021-11-29 05:26:18
////*  描    述：

////* ************************************************************************************
//using UnityEngine;
//using System.Collections.Generic;
//using UFrame.BgTransparent;
//using HighlightPlus;


//public class DiamondMaterialChangeCtrl
//{
//    protected Dictionary<int, RendererOperation> m_renderMaps;
//    protected const string COLOR_NAME = "_Color";
//    protected const string TEXTURE_NAME = "";
//    public const string WORP_COLOR_NAME = "_BaseColor";
//    public const string WORP_TEX_NAME = "";
//    public DiamondMaterialChangeCtrl()
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
//                renderOp = new RendererOperation(mat, new Renderer[] { renders }, COLOR_NAME, TEXTURE_NAME, WORP_COLOR_NAME, WORP_TEX_NAME);
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
