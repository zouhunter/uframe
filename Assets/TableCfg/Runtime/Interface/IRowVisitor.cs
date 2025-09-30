/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表行信息                                                                        *
*//************************************************************************************/
using System.Collections.Generic;
using System.IO;

namespace UFrame.TableCfg
{
    public interface IRowVisitor
    {
        object GetData(IRow row, string key);
    }

    public abstract class RowVisitor<T> :IRowVisitor where T:IRow
    {
        public object GetData(IRow row, string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return GetData((T)row, key);
        }

        public abstract object GetData(T row, string key);
    }
}
