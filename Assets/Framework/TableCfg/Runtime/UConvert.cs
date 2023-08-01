/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 表数据转换器                                                                    *
*//************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UFrame.TableCfg
{
    public class UConvert
    {
        private static UConvert _instance;
        public static UConvert Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UConvert();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public char array_separater_1 = '|';
        public char array_separater_2 = '$';

        private Dictionary<string, Func<string, object>> _strConvertMap;
        private Dictionary<string, Func<BinaryReaderContent, object>> _binaryConvertMap;
        private Dictionary<string, Action<object, StreamWriter>> _strConvertBackMap;
        private Dictionary<string, Action<object, BinaryWriter>> _binaryConvertBackMap;
        public Dictionary<string, Func<string, object>> strConvertMap
        {
            get
            {
                if (_strConvertMap == null)
                {
                    _strConvertMap = new Dictionary<string, Func<string, object>>();
                    InitStrConvertMap(_strConvertMap);

                }
                return _strConvertMap;
            }
        }
        public Dictionary<string, Func<BinaryReaderContent, object>> binaryConvertMap
        {
            get
            {
                if (_binaryConvertMap == null)
                {
                    _binaryConvertMap = new Dictionary<string, Func<BinaryReaderContent, object>>();
                    InitBinaryConvertMap(_binaryConvertMap);

                }
                return _binaryConvertMap;
            }
        }
        public Dictionary<string, Action<object, StreamWriter>> strConvertBackMap
        {
            get
            {
                if (_strConvertBackMap == null)
                {
                    _strConvertBackMap = new Dictionary<string, Action<object, StreamWriter>>();
                    InitStrConvertBackMap(_strConvertBackMap);
                }
                return _strConvertBackMap;
            }
        }
        public Dictionary<string, Action<object, BinaryWriter>> binaryConvertBackMap
        {
            get
            {
                if (_binaryConvertBackMap == null)
                {
                    _binaryConvertBackMap = new Dictionary<string, Action<object, BinaryWriter>>();
                    InitBinaryConvertBackMap(_binaryConvertBackMap);
                }
                return _binaryConvertBackMap;
            }
        }

        public static void Release()
        {
            _instance = null;
        }

        #region str to obj
        private void InitStrConvertMap(Dictionary<string, Func<string, object>> strConvertMap)
        {
            strConvertMap.Add(typeof(string).FullName, (x) => (x));
            strConvertMap.Add(typeof(byte).FullName, (x) => ToByte(x));
            strConvertMap.Add(typeof(sbyte).FullName, (x) => ToSByte(x));
            strConvertMap.Add(typeof(bool).FullName, (x) => ToBool(x));

            strConvertMap.Add(typeof(short).FullName, (x) => ToInt16(x));
            strConvertMap.Add(typeof(int).FullName, (x) => ToInt32(x));
            strConvertMap.Add(typeof(long).FullName, (x) => ToInt64(x));
            strConvertMap.Add(typeof(ushort).FullName, (x) => ToUInt16(x));
            strConvertMap.Add(typeof(uint).FullName, (x) => ToUInt32(x));
            strConvertMap.Add(typeof(ulong).FullName, (x) => ToUInt64(x));

            strConvertMap.Add(typeof(float).FullName, (x) => ToSingle(x));
            strConvertMap.Add(typeof(double).FullName, (x) => ToDouble(x));

            strConvertMap.Add(typeof(Vector2).FullName, (x) => ToVector2(x));
            strConvertMap.Add(typeof(Vector3).FullName, (x) => ToVector3(x));
            strConvertMap.Add(typeof(Vector4).FullName, (x) => ToVector4(x));
            strConvertMap.Add(typeof(Vector2Int).FullName, (x) => ToVector2Int(x));
            strConvertMap.Add(typeof(Vector3Int).FullName, (x) => ToVector3Int(x));
            strConvertMap.Add(typeof(Rect).FullName, (x) => ToRect(x));
            strConvertMap.Add(typeof(RectInt).FullName, (x) => ToRectInt(x));
            strConvertMap.Add(typeof(Color).FullName, (x) => ToColor(x));
#if !BINARY_CONFIG
            strConvertMap.Add(typeof(MString).FullName, (x) => ToMString(x));
            strConvertMap.Add(typeof(MString[]).FullName, (x) => ToMStringArray(x));
#endif
            strConvertMap.Add(typeof(int[]).FullName, (x) => ToInt32Array(x));
            strConvertMap.Add(typeof(List<int>).FullName, (x) => ToInt32List(x));
            strConvertMap.Add(typeof(float[]).FullName, (x) => ToSingleArray(x));
            strConvertMap.Add(typeof(string[]).FullName, (x) => ToStringArray(x));
            strConvertMap.Add(typeof(List<float>).FullName, (x) => ToSingleList(x));
            strConvertMap.Add(typeof(Vector3[]).FullName, (x) => ToVector3Array(x));
            strConvertMap.Add(typeof(string[][]).FullName, (x) => ToStringArrayArray(x));
            strConvertMap.Add(typeof(string[,]).FullName, (x) => ToStringDoubleArray(x));

            strConvertMap.Add(typeof(Dictionary<string, string>).FullName, (x) => ToDictionary<string, string>(x));
            strConvertMap.Add(typeof(Dictionary<int, int>).FullName, (x) => ToDictionary<string, string>(x));
            strConvertMap.Add(typeof(Dictionary<int, string>).FullName, (x) => ToDictionary<string, string>(x));
        }

        public object ChangeType(string value, string typeStr)
        {
            var type = System.Type.GetType(typeStr);
            if (type == null)
            {
                return null;
            }
            return Convert.ChangeType(value, type);
        }

        public T[] ToArray<T>(string value)
        {
            if (string.IsNullOrEmpty(value) || !strConvertMap.TryGetValue(typeof(T).FullName, out var conventer))
                return null;

            var strArray = value.Split(array_separater_1);
            if (typeof(T) == typeof(string))
            {
                return strArray as T[];
            }
            else
            {
                var arr = new T[strArray.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = (T)conventer?.Invoke(strArray[i]);
                }
                return arr;
            }
        }

        public string[] ToStringArray(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return value.Split(array_separater_1);
        }


        public string[] ToStringArray(string value, char separater)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return value.Split(separater);
        }

        public Vector2 ToVector2(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Vector2 v2 = Vector2.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 2; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    float.TryParse(array[i], out var result);
                    v2[i] = result;
                }
            }
            return v2;
        }

        public Vector3 ToVector3(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Vector3 v3 = Vector3.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 3; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    float.TryParse(array[i], out var result);
                    v3[i] = result;
                }
            }
            return v3;
        }

        public Vector4 ToVector4(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Vector4 vc = Vector4.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 4; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    float.TryParse(array[i], out var result);
                    vc[i] = result;
                }
            }
            return vc;
        }
        public Vector2Int ToVector2Int(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Vector2Int v2 = Vector2Int.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 2; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    int.TryParse(array[i], out var result);
                    v2[i] = result;
                }
            }
            return v2;
        }

        public Vector3Int ToVector3Int(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Vector3Int v3 = Vector3Int.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 3; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    int.TryParse(array[i], out var result);
                    v3[i] = result;
                }
            }
            return v3;
        }

        public Rect ToRect(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Rect vc = Rect.zero;
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 4; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    float.TryParse(array[i], out var result);
                    if (i == 0) vc.x = result;
                    else if (i == 1) vc.y = result;
                    else if (i == 2) vc.width = result;
                    else if (i == 3) vc.height = result;
                }
            }
            return vc;
        }

        public RectInt ToRectInt(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            RectInt vc = new RectInt();
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 4; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    int.TryParse(array[i], out var result);
                    if (i == 0) vc.x = result;
                    else if (i == 1) vc.y = result;
                    else if (i == 2) vc.width = result;
                    else if (i == 3) vc.height = result;
                }
            }
            return vc;
        }
        public Color ToColor(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            Color vc = new Color(1, 1, 1, 1);
            if (array != null)
            {
                for (int i = 0; i < array.Length && i < 4; i++)
                {
                    if (string.IsNullOrEmpty(array[i]))
                        continue;
                    float.TryParse(array[i], out var result);
                    if (i == 0) vc.r = result;
                    else if (i == 1) vc.g = result;
                    else if (i == 2) vc.b = result;
                    else if (i == 3) vc.a = result;
                }
            }
            return vc;
        }
        public List<int> ToInt32List(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            if (array == null)
                return null;

            var intArray = new List<int>();
            for (int i = 0; i < array.Length; i++)
            {
                if (string.IsNullOrEmpty(array[i]))
                    continue;
                int.TryParse(array[i], out var result);
                intArray.Add(result);
            }
            return intArray;
        }

        public int[] ToInt32Array(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            if (array == null)
                return null;

            var intArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                if (string.IsNullOrEmpty(array[i]))
                    continue;
                int.TryParse(array[i], out var result);
                intArray[i] = result;
            }
            return intArray;
        }

#if !BINARY_CONFIG
        public MString[] ToMStringArray(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            if (array == null)
                return null;
            var arr = new MString[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                arr[i] = new MString(array[i]);
            }
            return arr;
        }
        public MString ToMString(string value)
        {
            return new MString(value);
        }
#endif

        public float[] ToSingleArray(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            if (array == null)
                return null;

            var resultArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                if (string.IsNullOrEmpty(array[i]))
                    continue;
                float.TryParse(array[i], out var result);
                resultArray[i] = result;
            }
            return resultArray;
        }
        public List<float> ToSingleList(string value)
        {
            var array = ToStringArray(value, array_separater_1);
            if (array == null)
                return null;

            var arr = new List<float>();
            for (int i = 0; i < array.Length; i++)
            {
                if (string.IsNullOrEmpty(array[i]))
                    continue;
                float.TryParse(array[i], out var result);
                arr.Add(result);
            }
            return arr;
        }

        public Vector3[] ToVector3Array(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var array1 = ToStringArray(value, array_separater_2);
            Vector3[] arrays = new Vector3[array1.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                arrays[i] = ToVector3(array1[i]);
            }
            return arrays;
        }

        public string[][] ToStringArrayArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var array1 = value.Split(array_separater_2);
            string[][] arrays = new string[array1.Length][];
            for (int i = 0; i < array1.Length; i++)
            {
                arrays[i] = ToStringArray(array1[i], array_separater_1);
            }
            return arrays;
        }

        public string[,] ToStringDoubleArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var array2 = ToStringArrayArray(value);
            if (array2.Length > 0)
            {
                string[,] array = new string[array2.GetLength(0), array2[0].Length];
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        if (array2[i].Length > j)
                        {
                            array[i, j] = array2[i][j];
                        }
                    }
                }
                return array;
            }

            return null;
        }

        public Hashtable ToHashtable(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var list = ToStringArray(value, array_separater_2);
            var dic = new Hashtable();
            for (int i = 0; i < list.Length; i++)
            {
                var pair = ToStringArray(value, array_separater_1);
                if (pair != null && pair.Length > 1)
                {
                    dic[pair[0]] = pair[1];
                }
                else
                {
                    Debug.LogError("error pair:" + value);
                }
            }
            return dic;
        }

        public Dictionary<T, V> ToDictionary<T, V>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var list = ToStringArray(value, array_separater_2);
            var dic = new Dictionary<T, V>();
            for (int i = 0; i < list.Length; i++)
            {
                var pair = ToStringArray(list[i], array_separater_1);
                if (pair != null && pair.Length > 1)
                {
                    T k = Parse<T>(pair[0]);
                    V v = Parse<V>(pair[1]);
                    if (k != null && v != null)
                    {
                        dic[k] = v;
                    }
                }
                else
                {
                    Debug.LogError("error pair:" + value);
                }
            }
            return dic;
        }
        public byte ToByte(string input)
        {
            if (Byte.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public sbyte ToSByte(string input)
        {
            if (SByte.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public short ToInt16(string input)
        {
            if (Int16.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public int ToInt32(string input)
        {
            if (Int32.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public long ToInt64(string input)
        {
            if (Int64.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public UInt16 ToUInt16(string input)
        {
            if (UInt16.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public uint ToUInt32(string input)
        {
            if (UInt32.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public ulong ToUInt64(string input)
        {
            if (ulong.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public bool ToBool(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            if (input == "1")
                return true;
            else if (input == "0")
                return false;
            try
            {
                return Convert.ToBoolean(input);
            }
            catch (Exception)
            {
                Debug.LogError("error bool string:" + input);
                return false;
            }
        }
        public float ToSingle(string input)
        {
            if (Single.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }
        public double ToDouble(string input)
        {
            if (double.TryParse(input, out var result))
            {
                return result;
            }
            return 0;
        }

        public T Parse<T>(string valueStr)
        {
            var value = Parse(valueStr, typeof(T));
            if (value != null)
            {
                return (T)value;
            }
            return default(T);
        }

        public object Parse(string valueStr, Type valueType)
        {
            if (string.IsNullOrEmpty(valueStr))
            {
                return null;
            }
            if (typeof(Enum).IsAssignableFrom(valueType))
            {
                return Enum.Parse(valueType, valueStr, true);
            }
            else if (typeof(IConvertible).IsAssignableFrom(valueType))
            {
                return Convert.ChangeType(valueStr, valueType);
            }
            Debug.LogError("convent failed!" + valueStr);
            return null;
        }
        #endregion str to obj

        #region binary to obj

        private void InitBinaryConvertMap(Dictionary<string, Func<BinaryReaderContent, object>> binaryConvertMap)
        {
            binaryConvertMap[typeof(string).FullName] = (x) => x.ReadString();
            binaryConvertMap[typeof(byte).FullName] = (x) => x.ReadByte();
            binaryConvertMap[typeof(sbyte).FullName] = (x) => x.ReadByte();
            binaryConvertMap[typeof(bool).FullName] = (x) => x.ReadBoolean();

            binaryConvertMap[typeof(short).FullName] = (x) => x.ReadInt16();
            binaryConvertMap[typeof(int).FullName] = (x) => x.ReadInt32();
            binaryConvertMap[typeof(long).FullName] = (x) => x.ReadInt64();
            binaryConvertMap[typeof(ushort).FullName] = (x) => x.ReadUInt16();
            binaryConvertMap[typeof(uint).FullName] = (x) => x.ReadUInt32();
            binaryConvertMap[typeof(ulong).FullName] = (x) => x.ReadUInt64();
            binaryConvertMap[typeof(float).FullName] = (x) => x.ReadSingle();
            binaryConvertMap[typeof(double).FullName] = (x) => x.ReadDouble();

            binaryConvertMap[typeof(Vector2).FullName] = (x) => ReadVector2(x);
            binaryConvertMap[typeof(Vector3).FullName] = (x) => ReadVector3(x);
            binaryConvertMap[typeof(Vector4).FullName] = (x) => ReadVector4(x);
            binaryConvertMap[typeof(Vector2Int).FullName] = (x) => ReadVector2Int(x);
            binaryConvertMap[typeof(Vector3Int).FullName] = (x) => ReadVector3Int(x);
            binaryConvertMap[typeof(Rect).FullName] = (x) => ReadRect(x);
            binaryConvertMap[typeof(RectInt).FullName] = (x) => ReadRectInt(x);
            binaryConvertMap[typeof(Color).FullName] = (x) => ReadColor(x);
            binaryConvertMap[typeof(MString).FullName] = (x) => ReadMString(x);

            binaryConvertMap[typeof(int[]).FullName] = (x) => ReadArray<int>(x);
            binaryConvertMap[typeof(List<int>).FullName] = (x) => ReadList<int>(x);
            binaryConvertMap[typeof(float[]).FullName] = (x) => ReadArray<float>(x);
            binaryConvertMap[typeof(string[]).FullName] = (x) => ReadArray<string>(x);
            binaryConvertMap[typeof(MString[]).FullName] = (x) => ReadArray<MString>(x);
            binaryConvertMap[typeof(List<float>).FullName] = (x) => ReadList<float>(x);
            binaryConvertMap[typeof(Vector3[]).FullName] = (x) => ReadVector3Array(x);
            binaryConvertMap[typeof(string[][]).FullName] = (x) => ReadStringArrayArray(x);
            binaryConvertMap[typeof(string[,]).FullName] = (x) => ReadStringDoubleArray(x);

            binaryConvertMap[typeof(Dictionary<string, string>).FullName] = (x) => ReadDictionary<string, string>(x);
            binaryConvertMap[typeof(Dictionary<int, int>).FullName] = (x) => ReadDictionary<string, string>(x);
            binaryConvertMap[typeof(Dictionary<int, string>).FullName] = (x) => ReadDictionary<string, string>(x);
        }
        public Vector2 ReadVector2(BinaryReader reader)
        {
            Vector2 v = Vector2.zero;
            v.x = reader.ReadSingle();
            v.y = reader.ReadSingle();
            return v;
        }
        public Vector3 ReadVector3(BinaryReader reader)
        {
            Vector3 v = Vector3.zero;
            v.x = reader.ReadSingle();
            v.y = reader.ReadSingle();
            v.z = reader.ReadSingle();
            return v;
        }
        public Vector4 ReadVector4(BinaryReader reader)
        {
            Vector4 v = Vector4.zero;
            for (int i = 0; i < 4; i++)
            {
                v[i] = reader.ReadSingle();
            }
            return v;
        }
        public Vector2Int ReadVector2Int(BinaryReader reader)
        {
            Vector2Int v = Vector2Int.zero;
            v.x = reader.ReadInt32();
            v.y = reader.ReadInt32();
            return v;
        }
        public Vector3Int ReadVector3Int(BinaryReader reader)
        {
            Vector3Int v = Vector3Int.zero;
            v.x = reader.ReadInt32();
            v.y = reader.ReadInt32();
            return v;
        }
        public Rect ReadRect(BinaryReader reader)
        {
            Rect v = Rect.zero;
            v.x = reader.ReadSingle();
            v.y = reader.ReadSingle();
            v.width = reader.ReadSingle();
            v.height = reader.ReadSingle();
            return v;
        }
        public RectInt ReadRectInt(BinaryReader reader)
        {
            RectInt v = new RectInt();
            v.x = reader.ReadInt32();
            v.y = reader.ReadInt32();
            v.width = reader.ReadInt32();
            v.height = reader.ReadInt32();
            return v;
        }
        public Color ReadColor(BinaryReader reader)
        {
            Color c = new Color(1, 1, 1, 1);
            c.r = reader.ReadSingle();
            c.g = reader.ReadSingle();
            c.b = reader.ReadSingle();
            c.a = reader.ReadSingle();
            return c;
        }
        public int[] ReadInt32Array(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new int[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = reader.ReadInt32();
                }
                return arr;
            }
            return null;
        }

        public MString[] ReadMStringArray(BinaryReaderContent reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new MString[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = ReadMString(reader);
                }
                return arr;
            }
            return null;
        }

        public MString ReadMString(BinaryReaderContent content)
        {
            var pos = content.target.BaseStream.Position;
            var mstring = new MString(content, (int)pos);
            var sb = content.ReadSByte();
            pos += 1;
            int size = sb & 0b01111111;
            if (sb < 0)
            {
                var sb2 = content.ReadByte();
                size += sb2 * 128;
                pos += 1;
            }
            pos += size;
            content.target.BaseStream.Seek(pos, SeekOrigin.Begin);
            return mstring;
        }

        public List<int> ReadInt32List(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new List<int>(length);
                for (int i = 0; i < length; i++)
                {
                    arr.Add(reader.ReadInt32());
                }
                return arr;
            }
            return null;
        }
        public float[] ReadSingleArray(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new float[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = reader.ReadSingle();
                }
                return arr;
            }
            return null;
        }
        public List<float> ReadSingleList(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new List<float>(length);
                for (int i = 0; i < length; i++)
                {
                    arr.Add(reader.ReadSingle());
                }
                return arr;
            }
            return null;
        }
        public string[] ReadStringArray(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length > 0)
            {
                var arr = new string[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = reader.ReadString();
                }
                return arr;
            }
            return null;
        }
        public Vector3[] ReadVector3Array(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length == 0)
                return null;

            var arr = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = ReadVector3(reader);
            }
            return arr;
        }

        public string[][] ReadStringArrayArray(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length == 0)
                return null;

            var arr = new string[length][];
            for (int i = 0; i < length; i++)
            {
                arr[i] = ReadStringArray(reader);
            }
            return arr;
        }
        public string[,] ReadStringDoubleArray(BinaryReader reader)
        {
            var array2 = ReadStringArrayArray(reader);
            if (array2.Length > 0)
            {
                string[,] array = new string[array2.GetLength(0), array2[0].Length];
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        if (array2[i].Length > j)
                        {
                            array[i, j] = array2[i][j];
                        }
                    }
                }
                return array;
            }
            return null;
        }

        public T[] ReadArray<T>(BinaryReaderContent reader)
        {
            if (binaryConvertMap.TryGetValue(typeof(T).FullName, out var readFunc1))
            {
                var length = reader.ReadByte();

                var arr = new T[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = (T)readFunc1.Invoke(reader);
                }
                return arr;
            }
            return null;
        }

        public List<T> ReadList<T>(BinaryReaderContent reader)
        {
            if (binaryConvertMap.TryGetValue(typeof(T).FullName, out var readFunc1))
            {
                var length = reader.ReadByte();
                var arr = new List<T>(length);
                for (int i = 0; i < length; i++)
                {
                    arr.Add((T)readFunc1.Invoke(reader));
                }
                return arr;
            }
            else
            {
                Debug.LogError("ReadList reader not registed!" + typeof(T).FullName);
            }
            return null;
        }

        public Dictionary<T, V> ReadDictionary<T, V>(BinaryReaderContent reader)
        {
            binaryConvertMap.TryGetValue(typeof(T).FullName, out var readFunc1);
            binaryConvertMap.TryGetValue(typeof(V).FullName, out var readFunc2);
            if (readFunc1 != null && readFunc2 != null)
            {
                var length = reader.ReadByte();
                var dic = new Dictionary<T, V>();
                for (int i = 0; i < length; i++)
                {
                    var key = readFunc1(reader);
                    var value = readFunc2(reader);
                    if (key != null)
                    {
                        dic[(T)key] = (V)value;
                    }
                }
            }
            else
            {
                Debug.LogError("ReadDictionary reader not registed type:" + typeof(T).FullName + " or " + typeof(V).FullName);
            }
            return null;
        }
        #endregion binary to obj

        #region obj to str
        private void InitStrConvertBackMap(Dictionary<string, Action<object, StreamWriter>> strConvertBackMap)
        {
            strConvertBackMap[typeof(string).FullName] = (x, y) => StrToCsvString(x.ToString(), y);
            strConvertBackMap[typeof(byte).FullName] = ValueToString;
            strConvertBackMap[typeof(sbyte).FullName] = ValueToString;
            strConvertBackMap[typeof(bool).FullName] = ValueToString;

            strConvertBackMap[typeof(short).FullName] = ValueToString;
            strConvertBackMap[typeof(int).FullName] = ValueToString;
            strConvertBackMap[typeof(long).FullName] = ValueToString;
            strConvertBackMap[typeof(ushort).FullName] = ValueToString;
            strConvertBackMap[typeof(uint).FullName] = ValueToString;
            strConvertBackMap[typeof(ulong).FullName] = ValueToString;
            strConvertBackMap[typeof(float).FullName] = ValueToString;
            strConvertBackMap[typeof(double).FullName] = ValueToString;

            strConvertBackMap[typeof(Vector2).FullName] = Vector2ToString;
            strConvertBackMap[typeof(Vector3).FullName] = Vector3ToString;
            strConvertBackMap[typeof(Vector4).FullName] = Vector4ToString;
            strConvertBackMap[typeof(Vector2Int).FullName] = Vector2IntToString;
            strConvertBackMap[typeof(Vector3Int).FullName] = Vector3IntToString;
            strConvertBackMap[typeof(Rect).FullName] = RectToString;
            strConvertBackMap[typeof(RectInt).FullName] = RectIntToString;
            strConvertBackMap[typeof(Color).FullName] = ColorToString;
#if !BINARY_CONFIG
            strConvertBackMap[typeof(MString).FullName] = (x, y) => StrToCsvString((MString)x, y);
            strConvertBackMap[typeof(MString[]).FullName] = (x, y) => ArrayToString<MString>(x, y);
#endif
            strConvertBackMap[typeof(int[]).FullName] = (x, y) => ArrayToString<int>(x, y);
            strConvertBackMap[typeof(List<int>).FullName] = (x, y) => ListToString<int>(x, y);
            strConvertBackMap[typeof(float[]).FullName] = (x, y) => ArrayToString<float>(x, y);
            strConvertBackMap[typeof(string[]).FullName] = (x, y) => ArrayToString<string>(x, y);
            strConvertBackMap[typeof(List<float>).FullName] = (x, y) => ListToString<float>(x, y);
            strConvertBackMap[typeof(Vector3[]).FullName] = (x, y) => ArrayToString<Vector3>(x, y);

            strConvertBackMap[typeof(Dictionary<string, string>).FullName] = (x, y) => DictionaryToString<string, string>(x, y);
            strConvertBackMap[typeof(Dictionary<int, int>).FullName] = (x, y) => DictionaryToString<int, int>(x, y);
            strConvertBackMap[typeof(Dictionary<int, string>).FullName] = (x, y) => DictionaryToString<int, string>(x, y);
        }

        public void StrToCsvString(string obj, StreamWriter sw)
        {
            var strValue = obj.ToString();
            if (strValue.Contains(","))
            {
                strValue = $"\"{strValue}\"";
            }
            sw.Write(strValue);
        }

        public void ValueToString(object obj, StreamWriter sw)
        {
            sw.Write(obj.ToString());
        }

        public void Vector2ToString(object vec, StreamWriter sw)
        {
            Vector2 v = (Vector2)vec;
            sw.Write(v.x);
            sw.Write(array_separater_1);
            sw.Write(v.y);
        }
        public void Vector3ToString(object vec, StreamWriter sw)
        {
            Vector3 v = (Vector3)vec;
            sw.Write(v.x);
            sw.Write(array_separater_1);
            sw.Write(v.y);
            sw.Write(array_separater_1);
            sw.Write(v.z);
        }
        public void Vector4ToString(object vec, StreamWriter sw)
        {
            Vector4 v = (Vector4)vec;
            for (int i = 0; i < 4; i++)
            {
                sw.Write(v[i]);
                if (i != 3)
                {
                    sw.Write(array_separater_1);
                }
            }
        }
        public void Vector2IntToString(object vec, StreamWriter sw)
        {
            Vector2Int v = (Vector2Int)vec;
            sw.Write(v.x);
            sw.Write(array_separater_1);
            sw.Write(v.y);
        }
        public void Vector3IntToString(object vec, StreamWriter sw)
        {
            Vector3Int v = (Vector3Int)vec;
            sw.Write(v.x);
            sw.Write(array_separater_1);
            sw.Write(v.y);
            sw.Write(array_separater_1);
            sw.Write(v.z);
        }
        public void RectToString(object vec, StreamWriter sw)
        {
            Rect r = (Rect)vec;
            sw.Write(r.x);
            sw.Write(array_separater_1);
            sw.Write(r.y);
            sw.Write(array_separater_1);
            sw.Write(r.width);
            sw.Write(array_separater_1);
            sw.Write(r.height);
        }
        public void RectIntToString(object vec, StreamWriter sw)
        {
            RectInt r = (RectInt)vec;
            sw.Write(r.x);
            sw.Write(array_separater_1);
            sw.Write(r.y);
            sw.Write(array_separater_1);
            sw.Write(r.width);
            sw.Write(array_separater_1);
            sw.Write(r.height);
        }
        public void ColorToString(object value, StreamWriter sw)
        {
            Color c = (Color)value;
            sw.Write(c.r);
            sw.Write(array_separater_1);
            sw.Write(c.g);
            sw.Write(array_separater_1);
            sw.Write(c.b);
            sw.Write(array_separater_1);
            sw.Write(c.a);
        }
        public void ArrayToString<T>(object value, StreamWriter sw)
        {
            var array = (T[])value;
            if (array == null)
                return;
            if (strConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvert))
            {
                for (int i = 0; i < array.Length; i++)
                {
                    subConvert.Invoke(array[i], sw);
                    if (i != array.Length - 1)
                        sw.Write(array_separater_1);
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName);
            }
        }
        public void ListToString<T>(object value, StreamWriter sw)
        {
            var array = (List<T>)value;
            if (array == null)
                return;
            if (strConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvert))
            {
                for (int i = 0; i < array.Count; i++)
                {
                    subConvert.Invoke(array[i], sw);
                    if (i != array.Count - 1)
                        sw.Write(array_separater_1);
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName);
            }
        }

        public void DictionaryToString<T, V>(object value, StreamWriter sw)
        {
            var dic = (Dictionary<T, V>)value;
            if (dic == null)
                return;
            if (strConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvertT) && strConvertBackMap.TryGetValue(typeof(V).FullName, out var subConvertV))
            {
                int count = 0;
                foreach (var pair in dic)
                {
                    subConvertT.Invoke(pair.Key, sw);
                    sw.Write(array_separater_1);
                    subConvertV.Invoke(pair.Value, sw);
                    if (++count < dic.Count)
                    {
                        sw.Write(array_separater_2);
                    }
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName + " or " + typeof(V).FullName);
            }
        }
        #endregion

        #region obj to binary
        private void InitBinaryConvertBackMap(Dictionary<string, Action<object, BinaryWriter>> binaryConvertBackMap)
        {
            binaryConvertBackMap[typeof(string).FullName] = WriteStringToBinary;
            binaryConvertBackMap[typeof(byte).FullName] = (x, y) => y.Write((byte)x);
            binaryConvertBackMap[typeof(sbyte).FullName] = (x, y) => y.Write((sbyte)x);
            binaryConvertBackMap[typeof(bool).FullName] = (x, y) => y.Write((bool)x);

            binaryConvertBackMap[typeof(short).FullName] = (x, y) => y.Write((short)x);
            binaryConvertBackMap[typeof(int).FullName] = (x, y) => y.Write((int)x);
            binaryConvertBackMap[typeof(long).FullName] = (x, y) => y.Write((long)x);
            binaryConvertBackMap[typeof(ushort).FullName] = (x, y) => y.Write((ushort)x);
            binaryConvertBackMap[typeof(uint).FullName] = (x, y) => y.Write((uint)x);
            binaryConvertBackMap[typeof(ulong).FullName] = (x, y) => y.Write((ulong)x);

            binaryConvertBackMap[typeof(float).FullName] = (x, y) => y.Write((float)x);
            binaryConvertBackMap[typeof(double).FullName] = (x, y) => y.Write((double)x);

            binaryConvertBackMap[typeof(Vector2).FullName] = (x, y) => WriteVector2ToBinary((Vector2)x, y);
            binaryConvertBackMap[typeof(Vector3).FullName] = (x, y) => WriteVector3ToBinary((Vector3)x, y);
            binaryConvertBackMap[typeof(Vector4).FullName] = (x, y) => WriteVector4ToBinary((Vector4)x, y);
            binaryConvertBackMap[typeof(Vector2Int).FullName] = (x, y) => WriteVector2IntToBinary((Vector2Int)x, y);
            binaryConvertBackMap[typeof(Vector3Int).FullName] = (x, y) => WriteVector3IntToBinary((Vector3Int)x, y);
            binaryConvertBackMap[typeof(Rect).FullName] = (x, y) => WriteRectToBinary((Rect)x, y);
            binaryConvertBackMap[typeof(RectInt).FullName] = (x, y) => WriteRectIntToBinary((RectInt)x, y);
            binaryConvertBackMap[typeof(MString).FullName] = (x, y) => WriteStringToBinary((string)x, y);

            binaryConvertBackMap[typeof(int[]).FullName] = (x, y) => WriteArrayToBinary<int>((int[])x, y);
            binaryConvertBackMap[typeof(List<int>).FullName] = (x, y) => WriteListToBinary<int>((List<int>)x, y);
            binaryConvertBackMap[typeof(float[]).FullName] = (x, y) => WriteArrayToBinary<float>((float[])x, y);
            binaryConvertBackMap[typeof(string[]).FullName] = (x, y) => WriteArrayToBinary<string>((string[])x, y);
            binaryConvertBackMap[typeof(List<float>).FullName] = (x, y) => WriteListToBinary<float>((List<float>)x, y);
            binaryConvertBackMap[typeof(Vector3[]).FullName] = (x, y) => WriteArrayToBinary<Vector3>((Vector3[])x, y);

            binaryConvertBackMap[typeof(Dictionary<string, string>).FullName] = (x, y) => WriteDictionaryToBinary<string, string>(x, y);
            binaryConvertBackMap[typeof(Dictionary<int, int>).FullName] = (x, y) => WriteDictionaryToBinary<int, int>(x, y);
            binaryConvertBackMap[typeof(Dictionary<int, string>).FullName] = (x, y) => WriteDictionaryToBinary<int, string>(x, y);
        }

        public void WriteStringToBinary(object value, BinaryWriter writer)
        {
            if (value == null)
                writer.Write("");
            else
                writer.Write(value.ToString());
        }

        public void WriteVector2ToBinary(Vector2 value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }
        public void WriteVector3ToBinary(Vector3 value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }
        public void WriteVector4ToBinary(Vector4 value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }
        public void WriteVector2IntToBinary(Vector2Int value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }
        public void WriteVector3IntToBinary(Vector3Int value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }
        public void WriteRectToBinary(Rect value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.width);
            writer.Write(value.height);
        }
        public void WriteRectIntToBinary(RectInt value, BinaryWriter writer)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.width);
            writer.Write(value.height);
        }
        public void WriteByteArrayToBinary(byte[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write((byte)array[i]);
            }
        }

        public void WriteUInt32ArrayToBinary(uint[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public void WriteInt32ArrayToBinary(int[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public void WriteMStringArrayToBinary(MString[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public void WriteSingleArrayToBinary(float[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }
        public void WriteStringArrayToBinary(string[] array, BinaryWriter writer)
        {
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public void WriteArrayToBinary<T>(T[] value, BinaryWriter writer)
        {
            var array = value;
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }

            if (binaryConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvert))
            {
                writer.Write((byte)array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    subConvert.Invoke(array[i], writer);
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName);
            }
        }

        public void WriteListToBinary<T>(List<T> value, BinaryWriter writer)
        {
            var array = value;
            if (array == null)
            {
                writer.Write((byte)0);
                return;
            }

            if (binaryConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvert))
            {
                writer.Write((byte)array.Count);
                for (int i = 0; i < array.Count; i++)
                {
                    subConvert.Invoke(array[i], writer);
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName);
            }
        }
        public void WriteDictionaryToBinary<T, V>(object value, BinaryWriter writer)
        {
            var dic = (Dictionary<T, V>)value;
            if (dic == null)
            {
                writer.Write((byte)0);
                return;
            }
            if (binaryConvertBackMap.TryGetValue(typeof(T).FullName, out var subConvertT) && binaryConvertBackMap.TryGetValue(typeof(V).FullName, out var subConvertV))
            {
                writer.Write((byte)dic.Count);
                foreach (var pair in dic)
                {
                    subConvertT.Invoke(pair.Key, writer);
                    subConvertV.Invoke(pair.Value, writer);
                }
            }
            else
            {
                throw new TableException("strConvertBackMap not reg type:" + typeof(T).FullName + " or " + typeof(V).FullName);
            }
        }
        #endregion
    }
}