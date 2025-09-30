/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   时间相关工具。                                                                *
*//************************************************************************************/

using System;

namespace UFrame
{
    public static class TimeTool
    {
        #region 时间ticks
        /// <summary>
        /// Gets the time ticks.获取一个int的时间，一天内的，毫秒计算
        /// </summary>
        /// <returns>The time ticks.</returns>
        public static long GetTimeTicks()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static int GetTimeTicksSecond()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)System.Convert.ToInt64(ts.TotalSeconds);
        }
        #endregion

        #region   时间戳

        /// <summary>
        ///  获取时间戳（毫秒）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToLongMS(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        /// <summary>
        ///  获取时间戳（秒）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToLongS(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 将时间戳转换为DateTime
        /// </summary>
        /// <param time="longtime">时间戳单位秒</param>
        /// <returns></returns>
        public static DateTime ConvertIntToDateTime(int time)
        {
            DateTime startTime = new System.DateTime(1970, 1, 1);
            return startTime.AddTicks(time);
        }
        #endregion
        //UTC时间搓转为本地时间搓
        public static string ConvertUTCDateTime(double d, string v)
        {
            DateTime dt = DateTime.UtcNow;
            var localDt = DateTime.Now;
            var m = dt - localDt;
            var s = m.TotalMilliseconds;
            var ld = d - s;
            return ld.ToString();
        }
    }
}