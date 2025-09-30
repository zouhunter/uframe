/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 连接点数据                                                                      *
*//************************************************************************************/

using UnityEngine;
using System;

namespace UFrame.NodeGraph.DataModel
{
    [Serializable]
    public class ConnectionPointData
    {
        [SerializeField] protected string id;
        [SerializeField] protected string label;
        [SerializeField] protected string parentId;
        [SerializeField] protected int isInput;
        [SerializeField] protected Rect buttonRect;
        [SerializeField] protected int max;//最大连接数
        [SerializeField] protected string type;//类型

        public string Id => id;
        public ref string Label =>ref label;
        public ref string Type => ref type;
        public ref int Max => ref max;
        public string ParentId => parentId;
        public bool IsInput => isInput == 1;
        public bool IsOutput => isInput == 0;
        public ref Rect Region => ref buttonRect;

        public ConnectionPointData(string label, string type, int max, NodeData parent, bool isInput)
        {
            this.id = Guid.NewGuid().ToString();
            this.label = label;
            this.parentId = parent.Id;
            this.isInput = isInput ? 1 : 0;
            this.max = max;
            this.type = type;
        }

        public ConnectionPointData(ConnectionPointData p)
        {
            this.id = p.id;
            this.label = p.label;
            this.parentId = p.parentId;
            this.isInput = p.isInput;
            this.buttonRect = p.buttonRect;
            this.max = p.max;
            this.type = p.type;
        }

        // returns rect for outside marker
        public Rect GetGlobalRegion(Rect baseRect)
        {
            return new Rect(
                baseRect.x + buttonRect.x,
                baseRect.y + buttonRect.y,
                buttonRect.width,
                buttonRect.height
            );
        }

        // returns rect for connection dot
        public Vector2 GetGlobalPosition(Rect baseRect)
        {
            var x = 0f;
            var y = 0f;

            if (IsInput)
            {
                x = baseRect.x + 8f;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            if (IsOutput)
            {
                x = baseRect.x + baseRect.width;
                y = baseRect.y + buttonRect.y + (buttonRect.height / 2f) - 1f;
            }

            return new Vector2(x, y);
        }
    }
}
