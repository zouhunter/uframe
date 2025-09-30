/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定辅助脚本                                                                    *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace UFrame.LitUI
{
    public static class UGUIBindingHelper
    {
        /// <summary>
        /// 输入框
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="inputField"></param>
        /// <param name="sourceName"></param>
        public static void BindingInputField(this VMBinder Binder, InputField inputField, string sourceName)
        {
            Binder.RegistValueChange<string>(x => inputField.text = x, sourceName);
            Binder.RegistValueEvent(inputField.onValueChanged, sourceName);
        }
        /// <summary>
        /// 文本框
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="inputField"></param>
        /// <param name="sourceName"></param>
        public static void BindingText(this VMBinder Binder, Text textComponent, string sourceName)
        {
            Binder.RegistValueChange<string>(x => textComponent.text = x, sourceName);
        }
        /// <summary>
        /// 滑动条
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="sliderComponent"></param>
        /// <param name="sourceName"></param>
        public static void BindingSlider(this VMBinder Binder, Slider sliderComponent, string sourceName)
        {
            Binder.RegistValueChange<float>(x => sliderComponent.value = x, sourceName);
            Binder.RegistValueEvent(sliderComponent.onValueChanged, sourceName);
        }
        /// <summary>
        /// 下拉框
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="dropdown"></param>
        /// <param name="sourceName"></param>
        public static void BindingDropdown(this VMBinder Binder, Dropdown dropdown, string sourceName)
        {
            Binder.RegistValueChange<int>(x => dropdown.value = x, sourceName);
            Binder.RegistValueEvent(dropdown.onValueChanged, sourceName);
        }

        /// <summary>
        /// 下拉框
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="button"></param>
        /// <param name="sourceName"></param>
        public static void BindingButton(this VMBinder Binder, Button button, string sourceName)
        {
            Binder.RegistEvent(button.onClick, sourceName);
        }

        /// <summary>
        /// 双选
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="toggle"></param>
        /// <param name="sourceName"></param>
        public static void BindingToggle(this VMBinder Binder, Toggle toggle, string sourceName)
        {
            Binder.RegistValueChange<bool>(x=> toggle.isOn = x, sourceName);
            Binder.RegistValueEvent(toggle.onValueChanged, sourceName);
        }

        /// <summary>
        /// RawImage
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="rawImage"></param>
        /// <param name="sourceName"></param>
        public static void BindingRawImage(this VMBinder Binder,RawImage rawImage, string sourceName)
        {
            Binder.RegistValueChange<Texture>(x => rawImage.texture = x, sourceName);
        }

        /// <summary>
        /// Image
        /// </summary>
        /// <param name="Binder"></param>
        /// <param name="image"></param>
        /// <param name="sourceName"></param>
        public static void BindingImage(this VMBinder Binder, Image image, string sourceName)
        {
            Binder.RegistValueChange<Sprite>(x => image.sprite = x, sourceName);
        }
    }
}