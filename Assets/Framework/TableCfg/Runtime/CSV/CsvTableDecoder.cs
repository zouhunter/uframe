using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jagat.TableCfg
{
    public class CsvTableDecoder : ITableDecoder
    {
        public Func<int, object> GetRowProcess { get; set; }
        public Func<IRow> RowCreater { get; set; }

        public virtual AsyncTableOperation Decode(ITable table, StreamContent stream, System.Text.Encoding encoding)
        {
            var opeation = new AsyncTableOperation();
            var reader = GetRowProcess?.Invoke(typeof(ILineRowReader).GetHashCode());
            if (reader == null)
            {
                UnityEngine.Debug.LogException(new TableException("not ILineRowReader reader reg!"));
                return opeation;
            }

            var lineSerializer = reader as ILineRowReader;
            CsvHelper.Instance.ReadCSV(stream, encoding, (lineIndex, dataLine,finish) =>
            {
                try
                {
                    if (0 == lineIndex)
                    {
                        lineSerializer.SetFields(dataLine.ToArray());
                    }
                    else
                    {
                        var row = RowCreater?.Invoke()?? table.CreateRow();
                        lineSerializer.SetActiveRow(row);
                        lineSerializer.ReadDatas(dataLine);
                        table.AddRow(row);
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
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
