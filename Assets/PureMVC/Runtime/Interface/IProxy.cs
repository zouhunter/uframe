/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 包接口                                                                      *
*//************************************************************************************/

namespace UFrame
{
    public interface IProxy
    {
    }
    public interface IProxy<T> : IProxy
    {
        T Data { get; set; }
    }
    public interface IProxy< T1, T2> : IProxy
    {
        T1 Data1 { get; set; }
        T2 Data2 { get; set; }
    }
    public interface IProxy< T1, T2, T3> : IProxy
    {
        T1 Data1 { get; set; }
        T2 Data2 { get; set; }
        T3 Data3 { get; set; }
    }
}
