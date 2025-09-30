/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2025-02-08                                                                   *
*  版本: master                                                                       *
*  功能:                                                                              *
*   - bool遮罩                                                                        *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame
{
    public class BoolMask
    {
        public bool state { get; private set; }
        public event Action<bool> StateChangEvent;

        private HashSet<string> _masks = new HashSet<string>();

        public void AddMask(string flag)
        {
            _masks.Add(flag);
            RefreshState();
        }

        public void DelMask(string flag)
        {
            _masks.Remove(flag);
            RefreshState();
        }

        private void RefreshState()
        {
            var nextState = _masks.Count > 0;
            if (state != nextState)
            {
                state = nextState;
                StateChangEvent?.Invoke(state);
            }
        }
    }
}
