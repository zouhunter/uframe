/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - table 数据模型                                                                  *
*//************************************************************************************/

using System.Collections.Generic;
using System;

namespace UFrame.TableCfg
{
    [System.Serializable]
    public class TableData : IDisposable
    {
        public string name { get; set; }
        public List<List<string>> Rows = new List<List<string>>();

        public void Dispose()
        {
            Rows.Clear();
        }

        public string this[int col, int row]
        {
            get
            {
                if (Rows == null || Rows.Count <= row)
                {
                    return null;
                }
                var line = Rows[row];
                if (line == null || line.Count <= col)
                {
                    return null;
                }
                return line[col];
            }
        }
    }
}
