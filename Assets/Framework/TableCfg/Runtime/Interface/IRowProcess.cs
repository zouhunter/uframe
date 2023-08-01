/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表行信息                                                                        *
*//************************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jagat.TableCfg
{
    public interface IRowProcess
    {
        void SetFields(string[] fields);
        void SetActiveRow(IRow row);
    }
    public interface ILineRowWriter : IRowProcess
    {
        void WriteData(string key, StreamWriter sw);
    }

    public interface IBinaryRowWriter : IRowProcess
    {
        void WriteDatas(BinaryWriter writer);
    }

    public interface IBinaryRowReader : IRowProcess
    {
        string[] ReadFields(BinaryReaderContent reader);
        void ReadDatas(BinaryReaderContent reader);
    }

    public interface ILineRowReader : IRowProcess
    {
        void ReadDatas(List<string> values);
    }

    public abstract class RowProcess<T>:IRowProcess
    {
        protected T row;
        protected string[] fields;
        public virtual void SetFields(string[] fields)
        {
            this.fields = fields;
        }
        public virtual void SetActiveRow(IRow row)
        {
            this.row = (T)row;
        }
    }

    public abstract class LineRowReader<T> : RowProcess<T>, ILineRowReader where T : IRow
    {
        public abstract void ReadDatas(List<string> values);
    }

    public abstract class BinaryRowReader<T> : RowProcess<T>, IBinaryRowReader where T : IRow
    {
        public abstract void ReadDatas(BinaryReaderContent reader);

        public virtual string[] ReadFields(BinaryReaderContent reader)
        {
            var fieldNum = reader.ReadByte();
            var fields = new List<string>();
            for (int i = 0; i < fieldNum; i++)
            {
                fields.Add(reader.ReadString());
            }
            return fields.ToArray();
        }
    }

    public abstract class BinaryRowWriter<T> : RowProcess<T>, IBinaryRowWriter
    {
        public abstract void WriteDatas(BinaryWriter writer);
    }

    public abstract class LineRowWriter<T> : RowProcess<T>, ILineRowWriter
    {
        public abstract void WriteData(string key, StreamWriter sw);
    }
}
