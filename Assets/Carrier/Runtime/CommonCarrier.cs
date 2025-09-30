/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 通用载具                                                                        *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Carrier
{
    public class CommonCarrier:ICarrier {

        protected Transform body;
        protected Vector3 _startPos;
        protected Vector3 _targetPos;

        protected Vector3 startPos
        {
            get
            {
                return _startPos;
            }
        }

        protected Vector3 targetPos
        {
            get
            {
                return _targetPos;
            }
        }

        protected float speed
        {
            get
            {
                return 10;
            }
        }

        protected Vector3 currentPos
        {
            get
            {
                return body.transform.position;
            }
        }

        public bool Started { get; private set; }
        public void SetTargetPos(Vector3 targetPos)
        {
            this._targetPos = targetPos;
        }
        public void SetPassager(Transform body)
        {
            this.body = body;
            this._startPos = body.transform.position;
        }

        public bool JudgeArrived()
        {
            return Vector3.Distance(currentPos, targetPos) < 0.5f;
        }

        public void MoveStart()
        {
            Started = true;
        }

        public void MoveFrame()
        {
            body.LookAt(targetPos, Vector3.up);
            body.position = Vector3.MoveTowards(body.position, targetPos, speed * Time.deltaTime);
        }

        public void MoveComplete()
        {
            Started = false; 
        }
    }
}