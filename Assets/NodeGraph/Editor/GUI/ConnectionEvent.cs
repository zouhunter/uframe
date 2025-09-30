/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接事件                                                                        *
*//************************************************************************************/

namespace UFrame.NodeGraph
{
    public class ConnectionEvent
    {
        public enum EventType
        {
            EVENT_NONE,
            EVENT_CONNECTION_TAPPED,
            EVENT_CONNECTION_DELETED,
        }

        public readonly EventType eventType;
        public readonly ConnectionGUI eventSourceCon;

        public ConnectionEvent(EventType type, ConnectionGUI con)
        {
            this.eventType = type;
            this.eventSourceCon = con;
        }
    }
}