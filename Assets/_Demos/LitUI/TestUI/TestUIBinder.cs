///*************************************************************************************
///* 作    者：       
///* 创建时间：       2025-05-05 09:26:33
///* 说    明：       1.1.脚本由工具生成，
///                   2.2.可能被复写，
///                   3.3.请使用继承方式使用。
///* ************************************************************************************/

using UFrame.LitUI;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Weli
{
	/*绑定协议
	 * KEY_Button : Button0.interactable
	 * KEY_Button0Click : Button0.onClick
	 * KEY_InputField3_text : InputField3.text
	 * KEY_InputField3Endedit : InputField3.onEndEdit
	 * KEY_Txt2_text : Txt2.text
	 * Toggle_isOn : Toggle.isOn
	 * ToggleValuechanged : Toggle.onValueChanged
	 */
	[Preserve]
	public class TestUIBinder:VMBinder
	{
		public UnityEngine.UI.Button Button0;
		public UnityEngine.UI.Text Txt2;
		public UnityEngine.UI.InputField InputField3;
		public UnityEngine.UI.Toggle Toggle;

		/// <summary>
		/// 代码绑定
		/// </summary>
		public override void SetView(BindingView panel)
		{
			base.SetView(panel);
			
			#region Refs
			Button0 = GetRef<UnityEngine.UI.Button>("Button0");
			Txt2 = GetRef<UnityEngine.UI.Text>("Txt2");
			InputField3 = GetRef<UnityEngine.UI.InputField>("InputField3");
			Toggle = GetRef<UnityEngine.UI.Toggle>("Toggle");
			#endregion

			#region Props
			SetValue(Button0.interactable, "KEY_Button");
			RegistValueChange<bool>(x => Button0.interactable = x,"KEY_Button");
			SetValue(Txt2.text, "KEY_Txt2_text");
			RegistValueChange<string>(x => Txt2.text = x,"KEY_Txt2_text");
			SetValue(InputField3.text, "KEY_InputField3_text");
			RegistValueChange<string>(x => InputField3.text = x,"KEY_InputField3_text",InputField3.onEndEdit);
			SetValue(Toggle.isOn, "Toggle_isOn");
			RegistValueChange<bool>(x => Toggle.isOn = x,"Toggle_isOn",Toggle.onValueChanged);
			#endregion

			#region Events
			RegistEvent(Button0.onClick, "KEY_Button0Click");
			RegistEvent(InputField3.onEndEdit, "KEY_InputField3Endedit");
			RegistEvent(Toggle.onValueChanged, "ToggleValuechanged",Toggle);
			#endregion
		}

		/// <summary>
		/// 代码绑定
		/// </summary>
		public abstract class VM : ViewModel
		{
			protected Var<bool> m_KEY_Button = new ("KEY_Button");
			protected Evt m_KEY_Button0Click = new ("KEY_Button0Click");
			protected Var<string> m_KEY_Txt2_text = new ("KEY_Txt2_text");
			protected Var<string> m_KEY_InputField3_text = new ("KEY_InputField3_text");
			protected Evt<string> m_KEY_InputField3Endedit = new ("KEY_InputField3Endedit");
			protected Var<bool> m_Toggle_isOn = new ("Toggle_isOn");
			protected Evt<bool,UnityEngine.UI.Toggle> m_ToggleValuechanged = new ("ToggleValuechanged");
			public struct Binding
			{
				public UnityEngine.UI.Button Button0;
			}
			public Binding binding;
			public override void OnAfterBinding(BindingView panel)
			{
				base.OnAfterBinding(panel);
				var binder = panel.binder as TestUIBinder;
				binding.Button0 = binder.Button0;
			}
		}
	}
}
