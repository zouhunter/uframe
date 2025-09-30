//*************************************************************************************
//* 作    者： zht
//* 创建时间： 2023-05-08 10:08:58
//* 描    述：gb2312 header csv

//* ************************************************************************************
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UFrame.TableCfg
{
    public class OriginCsvTableDecoder : ITableDecoder
    {
        public AsyncTableOperation Decode(ITable table, StreamContent streamContent, Encoding encoding, Func<int, object> getRowProcess, Func<IRow> rowCreater)
        {
            var opeation = new AsyncTableOperation();
            var reader = getRowProcess?.Invoke(typeof(ILineRowReader).GetHashCode());
            if (reader == null)
            {
                Debug.LogError("not ILineRowReader reg!");
                return opeation;
            }
            var lineSerializer = reader as ILineRowReader;
            List<string> fields = new List<string>();
            CsvHelper.Instance.ReadCSV(streamContent, Encoding.GetEncoding("GB2312"), (lineIndex, dataLine, finish) =>
            {
                try
                {
                    if (1 == lineIndex)
                    {
                        for (int i = 0; i < dataLine.Count; i++)
                            fields.Add(dataLine[i].Replace("$", "").Trim());
                        lineSerializer.SetFields(fields.ToArray());
                    }
                    else if(lineIndex > 2)
                    {
                        var row = rowCreater?.Invoke() ?? table.CreateRow();
                        lineSerializer.SetActiveRow(row);
                        lineSerializer.ReadDatas(dataLine);
                        table.AddRow(row);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                if (finish)
                {
                    streamContent.Dispose();
                    opeation.SetFinish();
                }
            });
            return opeation;
        }

    }
}
#endif
