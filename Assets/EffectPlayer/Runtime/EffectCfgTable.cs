//*************************************************************************************
//* 作    者： 
//* 创建时间： 2022-07-06 10:19:26
//*  描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using UFrame.TableCfg;

namespace UFrame.TableCfg
{
    public class EffectCfgTable : ProxyTable<EffectCfgTable, EffectCfg>
    {
        public override string FileName => "effect_cfg";
    }
}