using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFrame.TableCfg
{
    public interface ITableEncoder
    {
        AsyncTableOperation Encode(string[] fields, List<IRow> rows, StreamContent streamContent, Func<int, object> rowProcess, Encoding encoding);
    }
}
