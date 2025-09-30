
using UnityEngine;
using UnityEngine.UI;

namespace UFrame.BridgeUI.Chart
{
    public interface IBarChartFactory
    {
        VertexHelper DrawBarChart( VertexHelper vh , Rect rect , sgSettingBase baseSetting ,BarChartSetting barChartSetting, BarChartData data = null );
    }
}
