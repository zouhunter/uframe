using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UFrame.TableCfg
{
    public class BinaryTableDecoder : ITableDecoder
    {
        public AsyncTableOperation Decode(ITable table, StreamContent streamContent, Encoding encoding, Func<int, object> rowProcess, Func<IRow> rowCreater)
        {
            var operation = new AsyncTableOperation();
            var binaryRowReaderObj = rowProcess?.Invoke(typeof(IBinaryRowReader).GetHashCode());
            if (binaryRowReaderObj == null)
            {
                Debug.LogError("no IBinaryRowReader reg!" + table.Name);
                operation.SetFinish();
                return operation;
            }
            var binaryReader = binaryRowReaderObj as IBinaryRowReader;
            using (var reader = new BinaryReaderContent(new BinaryReader(streamContent, encoding,true)))
            {
                streamContent.target.Seek(0,SeekOrigin.Begin);
                var fieldNum = reader.ReadByte();
                var fields = new List<string>();
                for (int i = 0; i < fieldNum; i++)
                {
                    fields.Add(reader.ReadString());
                }
                table.Fields = fields.ToArray();
                binaryReader.SetFields(table.Fields);
                var lineNum = reader.ReadUInt16();
                table.SetCapacity(lineNum);
                while (lineNum-- > 0)
                {
                    var row = rowCreater?.Invoke() ?? table.CreateRow();
                    binaryReader.SetActiveRow(row);
                    binaryReader.ReadDatas(reader);
                    table.AddRow(row);
                }
                operation.SetFinish();
            }
            return operation;
        }
    }
}
