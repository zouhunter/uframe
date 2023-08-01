/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表                                                                              *
*//************************************************************************************/
using System.IO;

namespace Jagat.TableCfg
{
    public interface IRow
    {
        
    }

    public interface IRow<Key> : IRow
    {
        Key K1 { get; }
    }

    public interface IRow<Key1, Key2> : IRow<Key1>
    {
        Key2 K2 { get; }
    }

    public interface IRow<Key1, Key2, Key3> : IRow<Key1, Key2>
    {
        Key3 K3 { get; }
    }
}
