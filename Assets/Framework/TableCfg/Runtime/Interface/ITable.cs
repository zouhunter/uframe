/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表行信息                                                                        *
*//************************************************************************************/
using System.Collections.Generic;
using System.IO;

namespace Jagat.TableCfg
{
    public interface ITable
    {
        int Count { get; }
        string Name { get; set; }
        void SetCapacity(int rowNum);
        IRow CreateRow();
        void AddRow(IRow row);
        void SetRows(List<IRow> rows);
        void GetRows(List<IRow> rows);
        void Clear();
    }

}
