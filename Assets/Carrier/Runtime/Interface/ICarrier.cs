/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 载具接口                                                                        *
*//************************************************************************************/

namespace UFrame.Carrier
{
    public interface ICarrier
    {
        bool Started { get; }
        bool JudgeArrived();
        void MoveStart();
        void MoveFrame();
        void MoveComplete();
    }
}
