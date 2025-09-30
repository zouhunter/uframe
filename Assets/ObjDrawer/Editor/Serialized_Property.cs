using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Internal;
using System.Reflection;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace UFrame.ObjDrawer
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Serialized_property
    {
        internal Serialized_object m_SerializedObject;
        public Serialized_object serializedObject
        {
            get
            {
                return this.m_SerializedObject;
            }
            set
            {
                m_SerializedObject = value;
            }
        }
        public Serialized_property parentProp { get; private set; }

        public FieldInfo fieldInfo;

        private object content;

        private List<Serialized_property> subProps;

        public object value { get { return fieldInfo.GetValue(content); }set { fieldInfo.SetValue(content,value); } }

        public bool isExpanded = true;

        public string displayName { get { return name; } }

        public string name { get { return fieldInfo.Name; } }

        public string type { get { return fieldInfo.FieldType.ToString(); } }

        public string tooltip { get; set; }

        public int depth { get; set; }

        public string propertyPath { get; private set; }

        internal int hashCodeForPropertyPathWithoutArrayIndex;

        public bool editable { get { return true; } }

        public bool isAnimated;


        public bool hasChildren { get { return subProps != null && subProps.Count > 0; } }

        public bool hasVisibleChildren { get { return hasChildren && subProps.Find(x => x.editable) != null; } }

        public Serialized_propertyType propertyType { get; private set; }

        public int intValue { get { return (propertyType == Serialized_propertyType.Integer) ? (int)value : 0; } }

        public long longValue { get { return (propertyType == Serialized_propertyType.Integer) ? (long)value : 0; } }

        public bool boolValue { get { return (propertyType == Serialized_propertyType.Boolean) ? (bool)value : false; } }

        public float floatValue { get { return (propertyType == Serialized_propertyType.Float) ? (float)value : 0; } }

        public double doubleValue { get { return (propertyType == Serialized_propertyType.Float) ? (double)value : 0; } }

        public string stringValue { get { return (propertyType == Serialized_propertyType.String) ? (string)value : null; } }

        public Color colorValue { get { return (propertyType == Serialized_propertyType.Color) ? (Color)value : Color.white; } } 

        public AnimationCurve animationCurveValue;

        internal Gradient gradientValue;

        public object objectReferenceValue;

        public int objectReferenceInstanceIDValue;

        internal string objectReferenceStringValue;

        internal string objectReferenceTypeString;

        internal string layerMaskStringValue;

        public int enumValueIndex;

        public string[] enumNames;
        public string[] enumDisplayNames;
        public Vector2 vector2Value;

        public Vector3 vector3Value;

        public Vector4 vector4Value { get { return new Vector4(); } }

        public Quaternion quaternionValue { get { return new Quaternion(); } }

        public Rect rectValue { get { return new Rect(); } }

        public Bounds boundsValue { get { return new Bounds(); } }

        public bool isArray { get { return fieldInfo.FieldType.IsArrayOrList(); } }

        public int arraySize;

        internal Serialized_property(FieldInfo info, object holder)
        {
            this.fieldInfo = info;
            this.content = holder;
            this.propertyType = JudgePropertyType(info);
            subProps = GetSubPropopertys(fieldInfo, holder);
        }
        ~Serialized_property()
        {
            this.Dispose();
        }
        public void SetParentProperty(Serialized_property parent)
        {
            this.parentProp = parent;
            if (!string.IsNullOrEmpty(parent.propertyPath))
            {
                propertyPath = parent.propertyPath + "/" + fieldInfo.Name;
            }
            else
            {
                propertyPath = fieldInfo.Name;
            }
        }

        public List<Serialized_property> GetSubPropopertys(FieldInfo field, object holder)
        {
            List<Serialized_property> list = new List<UFrame.ObjDrawer.Serialized_property>();

            var type = field.FieldType;

            if (field.GetValue(holder) == null && type.IsClass && type  != typeof(string))
            {
                field.SetValue( holder , Activator.CreateInstance(type));
            }

            if(field.GetValue(holder) == null && type == typeof(string))
            {
                field.SetValue(holder, "");
            }

            if(type.IsClass && type != typeof(string))
            {
                var value = field.GetValue(holder);

                if(value != null)
                {
                    FieldInfo[] fields = value.GetType().GetFields(BindingFlags.GetField | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var item in fields)
                    {
                        if (!IsFieldNeed(item))
                        {
                            Debug.Log("ignore:" + item.Name);
                            continue;
                        }
                       
                        var prop = new Serialized_property(item, value);
                        prop.SetParentProperty(this);
                        list.Add(prop);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// �жϹ��ֶ��ܷ����л�
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static bool IsFieldNeed(FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;

            //�ų��ֵ�
            if (type.IsGenericType && type.Name.Contains("Dictionary`"))
            {
                return false;
            }

            //�ų��ǹ��б���
            if (fieldInfo.Attributes != FieldAttributes.Public)
            {
                var attrs = fieldInfo.GetCustomAttributes(false);
                if (attrs.Length == 0 || (attrs.Length > 0 && Array.Find(attrs, x => x is SerializeField) == null))
                {
                    return false;
                }
            }

            //�ų��ӿ�
            if (type.IsInterface)
            {
                return false;
            }

            //����type
            if (type.IsArray || type.IsGenericType)
            {
                if (type.IsGenericType)
                {
                    type = type.GetGenericArguments()[0];
                }
                else
                {
                    type = type.GetElementType();
                }
            }

            //�ų�������Ľӿ�
            if (type.IsInterface)
            {
                return false;
            }

            //�ų��������л�����
            if (type.IsClass)
            {
                if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    var atts = type.GetCustomAttributes(false);
                    var seri = Array.Find(atts, x => x is System.SerializableAttribute);
                    if (seri == null)
                    {
                        return false;
                    }
                }
            }

            //�ų����ñ���
            if (fieldInfo.Name.Contains("k__BackingField"))
            {
                return false;
            }

            return true;
        }

      

        public void Dispose() { }

        public static bool EqualContents(Serialized_property x, Serialized_property y) { return false; }

        internal void SetBitAtIndexForAllTargetsImmediate(int index, bool value) { }

        public Serialized_property Next(bool enterChildren)
        {
            if (enterChildren)
            {
                if (hasChildren)
                {
                    return subProps[0];
                }
                return null;
            }
            else
            {
                var currid = parentProp.subProps.IndexOf(this);
                if (currid < parentProp.subProps.Count - 1)
                {
                    return subProps[currid + 1];
                }
                return null;
            }
        }

        public Serialized_property NextVisible(bool enterChildren)
        {
            if (enterChildren)
            {
                if (hasVisibleChildren)
                {
                    for (int i = 0; i < subProps.Count; i++)
                    {
                        if (subProps[i].editable)
                        {
                            return subProps[i];
                        }
                    }
                }
                return null;
            }
            else
            {
                var currid = parentProp.subProps.IndexOf(this);
                if (currid < parentProp.subProps.Count)
                {
                    for (int i = currid + 1; i < parentProp.subProps.Count; i++)
                    {
                        if (parentProp.subProps[i].editable)
                        {
                            return parentProp.subProps[i];
                        }
                    }
                }
                return null ;
            }
        }

        public bool DuplicateCommand() { return false; }

        public bool DeleteCommand() { return false; }
        
        internal Serialized_property FindPropertyInternal(string propertyPath)
        {
            if (subProps != null)
            {
                var item = subProps.Find(x => x.propertyPath == propertyPath);
                if(item != null)
                {
                    return item;
                }
            }
            return null;
        }


        internal Serialized_property FindPropertyRelativeInternal(string propertyPath)
        {
            var propertyPath_full = this.propertyPath + "/" + propertyPath;
            return FindPropertyInternal(propertyPath_full);
        }

        internal int[] GetLayerMaskSelectedIndex() { return null; }

        internal string[] GetLayerMaskNames() { return null; }

        internal void ToggleLayerMaskAtIndex(int index) { }

        private bool GetArrayElementAtIndexInternal(int index) { return false; }

        private static Serialized_propertyType JudgePropertyType(FieldInfo info)
        {
            var propertyType = Serialized_propertyType.Generic;
            var type = info.FieldType;
            if (Type.Equals(type,typeof(int))|| Type.Equals(type, typeof(long))|| Type.Equals(type, typeof(short)))
            {
                propertyType = Serialized_propertyType.Integer;
            }
            else if (Type.Equals(type, typeof(bool)))
            {
                propertyType = Serialized_propertyType.Boolean;
            }
            else if (Type.Equals(type, typeof(float))|| Type.Equals(type, typeof(double))|| Type.Equals(type, typeof(decimal)))
            {
                propertyType = Serialized_propertyType.Float;
            }
            else if (Type.Equals(type, typeof(string)))
            {
                propertyType = Serialized_propertyType.String;
            }
            else if (Type.Equals(type, typeof(Color)))
            {
                propertyType = Serialized_propertyType.Color;
            }
            else if (Type.Equals(type, typeof(Enum)))
            {
                propertyType = Serialized_propertyType.Enum;
            }
            else if (Type.Equals(type, typeof(LayerMask)))
            {
                propertyType = Serialized_propertyType.LayerMask;
            }
            else if (Type.Equals(type, typeof(Vector2)))
            {
                propertyType = Serialized_propertyType.Vector2;
            }
            else if (Type.Equals(type, typeof(Vector3)))
            {
                propertyType = Serialized_propertyType.Vector3;
            }
            else if (Type.Equals(type, typeof(Vector4)))
            {
                propertyType = Serialized_propertyType.Vector4;
            }
            else if (Type.Equals(type, typeof(Quaternion)))
            {
                propertyType = Serialized_propertyType.Quaternion;
            }
            else if (Type.Equals(type, typeof(Rect)))
            {
                propertyType = Serialized_propertyType.Rect;
            }
            else if (Type.Equals(type, typeof(Bounds)))
            {
                propertyType = Serialized_propertyType.Bounds;
            }
            else if (type.IsArrayOrList())
            {
                propertyType = Serialized_propertyType.ArraySize;
            }
            else if(type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                propertyType = Serialized_propertyType.ObjectReference;
            }
            else if (Type.Equals(type, typeof(AnimationCurve)))
            {
                propertyType = Serialized_propertyType.AnimationCurve;
            }
            return propertyType;
        }

        public void InsertArrayElementAtIndex(int index) { }

        public void DeleteArrayElementAtIndex(int index) { }


        public void ClearArray() { }


        public bool MoveArrayElement(int srcIndex, int dstIndex) { return false; }

    }
}
