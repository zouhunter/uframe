///*************************************************************************************
///* 作    者：       
///* 创建时间：       2025-01-21 12:31:43
///* 说    明：       1.脚本由工具初步生成，
///                   2.不能再次生成覆盖.
///* ************************************************************************************/

using UFrame.LitUI;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Weli
{
    /// <summary>
    /// 模板
    /// <summary>
    [Preserve]
    public class TestUIModel : ViewModel
    {
        #region AUTO_UI_BINDING
        protected Var<bool> m_Button = new("Button");
        protected Evt m_Button0Click = new("Button0Click");
        protected Var<string> m_Txt2_text = new("Txt2_text");
        protected Var<string> m_InputField3_text = new("InputField3_text");
        protected Evt<string> m_InputField3Endedit = new("InputField3Endedit");
        #endregion AUTO_UI_BINDING

        public string temp;

        public TestUIModel()
        {
            m_Button0Click.Set(OnClick);
        }

        private void OnClick()
        {
            Debug.Log("onClick");
        }
    }
}
