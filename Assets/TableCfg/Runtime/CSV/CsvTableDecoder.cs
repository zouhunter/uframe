using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UFrame.TableCfg
{
    public class CsvTableDecoder : ITableDecoder
    {
        public virtual AsyncTableOperation Decode(ITable table, StreamContent stream, System.Text.Encoding encoding, Func<int, object> getRowProcesses, Func<IRow> rowCreater)
        {
            var opeation = new AsyncTableOperation();
            var reader = getRowProcesses?.Invoke(typeof(ILineRowReader).GetHashCode());
            if (reader == null)
            {
                UnityEngine.Debug.LogException(new TableException("not ILineRowReader reader reg!"));
                return opeation;
            }

            var lineSerializer = reader as ILineRowReader;
            CsvHelper.Instance.ReadCSV(stream, encoding, (lineIndex, dataLine,finish) =>
            {
                {
                    if (0 == lineIndex)
                    {
                        var fields = new string[dataLine.Count];
                        for (int i = 0; i < dataLine.Count; i++)
                            fields[i] = dataLine[i].Replace("$", "").Trim();
                        lineSerializer.SetFields(fields);
                        table.Fields = fields;
                    }
                    else
                    {
                        var row = rowCreater?.Invoke() ?? table.CreateRow();
                        lineSerializer.SetActiveRow(row);
                        lineSerializer.ReadDatas(dataLine);
                        table.AddRow(row);
                    }
                }

                if(finish)
                {
                    stream.Dispose();
                    opeation.SetFinish();
                }
            });
            return opeation;
        }
    }
}
