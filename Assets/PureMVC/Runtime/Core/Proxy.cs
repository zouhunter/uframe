/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 包                                                                              *
*//************************************************************************************/

namespace UFrame
{
    public class Proxy<T> : IProxy<T>
    {
        public Proxy()
        {
        }
        public Proxy(T data = default(T))
        {
            Data = data;
        }

        public virtual T Data { get; set; }
    }
    public class Proxy<T1, T2> : IProxy< T1, T2>
    {
        public Proxy(T1 data1 = default(T1), T2 data2 = default(T2))
        {
            Data1 = data1;
            Data2 = data2;
        }
        public T1 Data1 { get; set; }
        public T2 Data2 { get; set; }
    }
    public class Proxy<T1, T2, T3> : IProxy<T1, T2, T3>
    {
        public Proxy(T1 data1 = default(T1), T2 data2 = default(T2), T3 data3 = default(T3))
        {
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
        }
        public T1 Data1 { get; set; }
        public T2 Data2 { get; set; }
        public T3 Data3 { get; set; }
    }
}
