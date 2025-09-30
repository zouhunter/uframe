//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-02 10:03:42
//* 描    述：  

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.BridgeUI.Editors
{
    public class ComponentViewCoder : ViewCoderExecuter
    {
        public ComponentViewCoder(ViewCoder viewCoder) : base(viewCoder) { }

        public override void AnalysisBinding(GameObject gameObject, ComponentItem[] componentItems)
        {
            
        }

        public override string GenerateScript()
        {
            return "";
        }
    }
}