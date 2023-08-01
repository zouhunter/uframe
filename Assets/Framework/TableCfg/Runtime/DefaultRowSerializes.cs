using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System;

namespace Jagat.TableCfg
{
    public class DefaultRowProcess:IRowProcess
    {
        private Type lastType;
        public string[] fields { get; set; }

        public KeyValuePair<string, MemberInfo>[] memberList;
        public Dictionary<string, KeyValuePair<string, MemberInfo>> memberDic;
        protected IRow m_row;
        public void SetFields(string[] fields)
        {
            this.fields = fields;
        }
        public void SetActiveRow(IRow row)
        {
            Install(row.GetType());
            m_row = row;
        }
        private void Install(Type type)
        {
            if (type == lastType)
                return;
            lastType = type;
            memberDic = new Dictionary<string, KeyValuePair<string, MemberInfo>>();
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField);
            foreach (var field in fieldInfos)
            {
                memberDic[field.Name] = new KeyValuePair<string, MemberInfo>(field.FieldType.FullName, field);
            }
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            foreach (var prop in props)
            {
                if (prop.SetMethod != null)
                {
                    memberDic[prop.Name] = new KeyValuePair<string, MemberInfo>(prop.PropertyType.FullName, prop);
                }
            }
            memberList = new KeyValuePair<string, MemberInfo>[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var key = fields[i];
                if (memberDic.ContainsKey(key))
                {
                    memberList[i] = memberDic[key];
                }
                else
                {
                    throw new TableException("config member lost:" + fields[i] + " :" + memberDic.Count);
                }
            }
        }
    }

    public class DefaultLineRowReader : DefaultRowProcess, ILineRowReader
    {
        public void ReadDatas(List<string> dataline)
        {
            for (int i = 0; i < memberList.Length; i++)
            {
                var pair = memberList[i];
                var text = dataline[i];
                var typeName = pair.Key;
                if (UConvert.Instance.strConvertMap.TryGetValue(typeName, out var method))
                {
                    var obj = method?.Invoke(text);
                    if (obj != null)
                    {
                        if (pair.Value.MemberType == MemberTypes.Field)
                        {
                            (pair.Value as FieldInfo).SetValue(m_row, obj);
                        }
                        else if (pair.Value.MemberType == MemberTypes.Property)
                        {
                            (pair.Value as PropertyInfo).SetValue(m_row, obj);
                        }
                    }
                }
                else
                {
                    throw new TableException("UConvert.Instance.strConvertMap not exists:" + pair.Key);
                }
            }
        }
    }

    public class DefaultBinaryRowReader : DefaultRowProcess, IBinaryRowReader
    {
        public void ReadDatas(BinaryReaderContent reader)
        {
            foreach (var pair in memberList)
            {
                var typeName = pair.Key;
                if (UConvert.Instance.binaryConvertMap.TryGetValue(pair.Key, out var readerFunc))
                {
                    var obj = readerFunc?.Invoke(reader);
                    if (obj != null)
                    {
                        if (pair.Value.MemberType == MemberTypes.Field)
                        {
                            (pair.Value as FieldInfo).SetValue(m_row, obj);
                        }
                        else if (pair.Value.MemberType == MemberTypes.Property)
                        {
                            (pair.Value as PropertyInfo).SetValue(m_row, obj);
                        }
                    }
                }
                else
                {
                    throw new TableException("UConvert.Instance.binaryConvertMap not exists:" + pair.Key);
                }
            }
        }

        public string[] ReadFields(BinaryReaderContent readerContent)
        {
            var fieldNum = readerContent.ReadByte();
            var fields = new List<string>();
            for (int i = 0; i < fieldNum; i++)
            {
                fields.Add(readerContent.ReadString());
            }
            return fields.ToArray();
        }
    }

    public class DefaultLineRowWriter : DefaultRowProcess, ILineRowWriter
    {
        public void WriteData(string key, StreamWriter sb)
        {
            if (memberDic.TryGetValue(key, out var pair))
            {
                var typeName = pair.Key;
                if (UConvert.Instance.strConvertBackMap.TryGetValue(pair.Key, out var writeFunc))
                {
                    object obj = null;
                    if (pair.Value.MemberType == MemberTypes.Field)
                    {
                        obj = (pair.Value as FieldInfo).GetValue(m_row);
                    }
                    else if (pair.Value.MemberType == MemberTypes.Property)
                    {
                        obj = (pair.Value as PropertyInfo).GetValue(m_row);
                    }
                    if (obj != null)
                    {
                        writeFunc?.Invoke(obj, sb);
                    }
                }
                else
                {
                    throw new TableException("UConvert.Instance.strConvertBackMap not exists:" + pair.Key);
                }
            }
        }
    }

    public class DefaultBinaryRowWriter : DefaultRowProcess, IBinaryRowWriter
    {
        public void WriteDatas(BinaryWriter writer)
        {
            foreach (var pair in memberList)
            {
                var typeName = pair.Key;
                if (UConvert.Instance.binaryConvertBackMap.TryGetValue(pair.Key, out var writeFunc))
                {
                    object obj = null;
                    if (pair.Value.MemberType == MemberTypes.Field)
                    {
                        obj = (pair.Value as FieldInfo).GetValue(m_row);
                    }
                    else if (pair.Value.MemberType == MemberTypes.Property)
                    {
                        obj = (pair.Value as PropertyInfo).GetValue(m_row);
                    }
                    writeFunc?.Invoke(obj, writer);
                }
                else
                {
                    throw new TableException("UConvert.Instance.binaryConvertBackMap not exists:" + pair.Key);
                }
            }
        }
    }
}