using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UFrame.BehaviourTree
{
    public class GraphColorUtil
    {
        public static Color GetColorByState(byte status, bool frame)
        {
            Color color;
            switch (status)
            {
                case Status.Inactive:
                    color = Color.gray;
                    break;
                case Status.Running:
                    color = Color.yellow;
                    break;
                case Status.Failure:
                    color = Color.red;
                    break;
                case Status.Success:
                    color = Color.green;
                    break;
                case Status.Interrupt:
                    color = Color.blue;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            if (!frame)
            {
                // 与灰色混合，变暗淡
                Color gray = new Color(0.5f, 0.5f, 0.5f, 1f);
                color = Color.Lerp(gray, color, 0.4f); // 0.4为主色权重，可根据实际调整
                color.a = 1f; // 保持不透明
            }
            else
            {
                color.a = 1f;
            }
            return color;
        }

        public static void SetColorByState(UnityEngine.UI.Graphic target, byte status, bool frame)
        {
            if (!target)
                return;

            target.color = GetColorByState(status, frame);
        }
    }
}
