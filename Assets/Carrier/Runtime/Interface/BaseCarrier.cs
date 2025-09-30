/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 载具模板                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Carrier
{
    public abstract class BaseCarrier : ICarrier
    {
        protected abstract Vector3 startPos { get; }
        protected abstract Vector3 targetPos { get; }
        protected abstract float speed { get; }
        protected abstract Vector3 currentPos { get; }
        public bool Started { get; private set; }

        public bool JudgeArrived()
        {
            return Vector3.Distance(currentPos,targetPos) < 0.5f;
        }

        public void MoveFrame()
        {
            var dir = (targetPos - currentPos).normalized * speed * Time.deltaTime;
            OnMoveDirection(dir);
        }

        public void MoveStart()
        {
            Started = true;
            OnMoveStarted();
        }

        public void MoveComplete()
        {
            Started = false;
            OnMoveCompleted();
        }

        protected abstract void OnMoveStarted();
        protected abstract void OnMoveDirection(Vector3 dir);
        protected abstract void OnMoveCompleted();
    }
}