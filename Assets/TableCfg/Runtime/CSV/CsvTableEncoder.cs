using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFrame.TableCfg
{
    public class CsvTableEncoder : ITableEncoder
    {
        public AsyncTableOperation Encode(string[] fields, List<IRow> rows, StreamContent streamContent, Func<int, object> rowProcess, Encoding encoding)
        {
            var rowWriterObj = rowProcess?.Invoke(typeof(ILineRowWriter).GetHashCode());
            var rowWriter = (ILineRowWriter)rowWriterObj;
            rowWriter.SetFields(fields);
            using (var streamWriter = new StreamWriter(streamContent.target, Encoding.UTF8))
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i != 0)
                        streamWriter.Write(',');
                    streamWriter.Write(fields[i]);
                }
                for (int i = 0; i < rows.Count; i++)
                {
                    streamWriter.WriteLine();
                    var row = rows[i];
                    rowWriter.SetActiveRow(row);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        if (j != 0)
                            streamWriter.Write(',');
                        rowWriter.WriteData(fields[j], streamWriter);
                    }
                }
            }
            return AsyncTableOperation.Sync;
    }
    }
}
