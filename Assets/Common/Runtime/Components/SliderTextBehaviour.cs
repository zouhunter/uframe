using UnityEngine;
using UnityEngine.UI;

namespace UFrame
{
    public class SliderTextBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected Slider m_targetSlider;
        [SerializeField]
        protected Text m_text;
        [SerializeField]
        protected float m_muti = 1;
        [SerializeField]
        protected string m_format;

        private void Start()
        {
            m_targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            if (m_text)
            {
                value *= m_muti;
                var textValue = value.ToString();
                if (!string.IsNullOrEmpty(m_format))
                {
                    textValue = m_format.Replace("{0}", textValue);
                }
                m_text.text = textValue;
            }
        }
    }
}