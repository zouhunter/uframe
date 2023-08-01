using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace Jagat.TableCfg
{
    public class BinaryTableDecoder : ITableDecoder
    {
        public Func<int, object> GetRowProcess { get; set; }
        public Func<IRow> RowCreater { get; set; }

        public AsyncTableOperation Decode(ITable table, StreamContent streamContent, Encoding encoding)
        {
            var operation = new AsyncTableOperation();
            var binaryRowReaderObj = GetRowProcess?.Invoke(typeof(IBinaryRowReader).GetHashCode());
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
                binaryReader.SetFields(fields.ToArray());
                var lineNum = reader.ReadUInt16();
                table.SetCapacity(lineNum);
                while (lineNum-- > 0)
                {
                    var row = RowCreater?.Invoke() ?? table.CreateRow();
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