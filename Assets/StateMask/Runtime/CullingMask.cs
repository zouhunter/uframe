/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2025-02-08                                                                   *
*  版本: master                                                                       *
*  功能:                                                                              *
*   - Culling遮罩                                                                        *
*//************************************************************************************/

using System;
using System.Collections.Generic;

namespace UFrame
{
    public class CullingMask
    {
        public int state { get; private set; }
        public event Action<int> StateChangEvent;
        private int _fullMask;
        private Dictionary<string,int> _masks = new Dictionary<string, int>();

        public CullingMask(int fullMask = -1)
        {
            this._fullMask = fullMask;
            this.state = fullMask;
        }

        public void AddMask(string flag,int state)
        {
            _masks[flag] = state;
            RefreshState();
        }

        public void DelMask(string flag)
        {
            _masks.Remove(flag);
            RefreshState();
        }

        private void RefreshState()
        {
            var nextState = _fullMask;
            foreach (var pair in _masks)
            {
                nextState &= ~pair.Value;
            }
            if (state != nextState)
            {
                state = nextState;
                StateChangEvent?.Invoke(state);
            }
        }
    }
}
