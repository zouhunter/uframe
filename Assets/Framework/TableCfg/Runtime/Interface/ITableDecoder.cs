using System.Collections.Generic;
using System.IO;

namespace Jagat.TableCfg
{
    public interface ITableDecoder
    {
        System.Func<int, object> GetRowProcess { get; set; }
        System.Func<IRow> RowCreater { get; set; }
        AsyncTableOperation Decode(ITable table, StreamContent stream, System.Text.Encoding encoding);
    }

    public interface ITableEncoder
    {
        System.Func<int, object> GetRowProcess { get; set; }
        AsyncTableOperation Encode(string[] fields,List<IRow> rows, StreamContent stream, System.Text.Encoding encoding);
    }
}