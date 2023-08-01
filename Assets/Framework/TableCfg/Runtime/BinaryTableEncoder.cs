using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

namespace UFrame.TableCfg
{
    public class BinaryTableEncoder : ITableEncoder
    {
        public Func<int, object> GetRowProcess { get; set; }

        public AsyncTableOperation Encode(string[] fields, List<IRow> rows, StreamContent streamContent,System.Text.Encoding encoding)
        {
            var operation = new AsyncTableOperation();
            IBinaryRowWriter encoder = GetRowProcess?.Invoke(typeof(IBinaryRowWriter).GetHashCode()) as IBinaryRowWriter;
            if(encoder == null)
            {
                Debug.LogError("IBinaryRowWriter not exists!");
                return operation;
            }
            using (var writer = new BinaryWriter(streamContent.target, encoding))
            {
                writer.Write((byte)fields.Length);
                foreach (var item in fields)
                {
                    writer.Write(item ?? string.Empty);
                }
                writer.Write((ushort)rows.Count);
                encoder.SetFields(fields);
                foreach (IRow row in rows)
                {
                    encoder.SetActiveRow(row);
                    encoder.WriteDatas(writer);
                }
                operation.SetFinish();
            }
            return operation;
        }
    }
}