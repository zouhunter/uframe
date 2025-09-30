using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFrame.TableCfg
{
    public interface ITableDecoder
    {
        AsyncTableOperation Decode(ITable table, StreamContent stream, System.Text.Encoding encoding, Func<int, object> rowProcess = null, Func<IRow> rowCreater = null);
    }
}
