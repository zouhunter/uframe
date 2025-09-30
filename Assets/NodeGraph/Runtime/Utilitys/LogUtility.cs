/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 日志输出                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.NodeGraph
{

    public class LogUtility
    {

        public static readonly string kTag = "Node";

        private static Logger s_logger;

        public static Logger Logger
        {
            get
            {
                if (s_logger == null)
                {
#if UNITY_2017_1_OR_NEWER
                    s_logger = new Logger(Debug.unityLogger.logHandler);
#else
					s_logger = new Logger(Debug.logger.logHandler);
#endif
                }

                return s_logger;
            }
        }
    }
}
