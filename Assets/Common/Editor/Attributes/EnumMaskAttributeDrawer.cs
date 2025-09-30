//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2021-10-17 12:14:39
//* 描    述： 
//* ************************************************************************************

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UFrame
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskAttributeDrawer : PropertyDrawer
    {
        protected Dictionary<System.Type, List<KeyValuePair<string, int>>> m_InfoDic = new Dictionary<System.Type, List<KeyValuePair<string, int>>>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumMask = (attribute as EnumMaskAttribute);
            if (enumMask != null)
            {
                if (!m_InfoDic.TryGetValue(enumMask.classType, out var valueList))
                {
                    valueList = new List<KeyValuePair<string, int>>();
                    if (enumMask.classType.IsEnum)
                    {
                        var enumNames = enumMask.classType.GetEnumNames();
                        var enumValues = enumMask.classType.GetEnumValues();
                        var enumerator = enumValues.GetEnumerator();
                        int index = 0;
                        while (enumerator.MoveNext())
                        {
                            var value = (int)enumerator.Current;
                            valueList.Add(new KeyValuePair<string, int>(enumNames[index++], value));
                        }
                    }
                    else
                    {
                        var fields = enumMask.classType.GetFields();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var field = fields[i];
                            valueList.Add(new KeyValuePair<string, int>(field.Name, (int)field.GetValue(null)));
                        }
                    }
                    m_InfoDic[enumMask.classType] = valueList;
                }

                var result = EditorGUI.MaskField(position, label, Pack(valueList, property.intValue), valueList.Select(x => x.Key).ToArray());
                property.intValue = UnPack(valueList, result);
            }
        }
        protected int Pack(List<KeyValuePair<string, int>> fieldList, int value)
        {
            int packedValue = 0;
            for (int i = 0; i < fieldList.Count; i++)
            {
                var pair = fieldList[i];
                if ((1 << pair.Value & value) != 0)
                {
                    packedValue |= 1 << i;
                }
            }
            return packedValue;
        }
        protected int UnPack(List<KeyValuePair<string, int>> fieldList, int result)
        {
            int unPackedValue = 0;
            for (int i = 0; i < fieldList.Count; i++)
            {
                if ((result & 1 << i) != 0)
                {
                    unPackedValue |= 1 << fieldList[i].Value;
                }
            }
            return unPackedValue;
        }
    }
}