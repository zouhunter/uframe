/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 场景加载控制器                                                                        *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UFrame.SceneLoad
{
    public class GameSceneLoadCtrl : GameSceneLoadCtrlGeneric<int>, IUpdate, IAlive
    {
        public bool Alive { get; protected set; }
        public float Interval => 0;
        public bool Runing => m_updateableScenes.Count > 0;

        public virtual void Initialize()
        {
            Alive = true;
        }
        public override void Recover()
        {
            base.Recover();
            Alive = false;
        }

        public override void OnUpdate()
        {
            if (!Alive)
                return;
            base.OnUpdate();
        }
    }
}