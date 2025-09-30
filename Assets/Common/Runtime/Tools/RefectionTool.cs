/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   反射类相关工具。                                                                *
*//************************************************************************************/
using System.Reflection;
using System.Collections.Generic;

namespace UFrame
{
    public class RefectionTool
    {
        //自动填充对象字段id
        public static void InitSelfIncrement<T>(bool property = false)
        {
            if (property)
            {
                var props = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.SetProperty);
                for (int i = 0; i < props.Length; i++)
                {
                    try
                    {
                        props[i].SetValue(null, i);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
            else
            {
                var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
                for (int i = 0; i < fields.Length; i++)
                {
                    try
                    {
                        fields[i].SetValue(null, i);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
        }

        //自动填充对象字段id并返回字典
        public static Dictionary<int, string> InitSelfIncrementWithNameDict<T>(bool property = false)
        {
            if (property)
            {
                var props = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty);
                var eventNameDic = new Dictionary<int, string>();
                for (int i = 0; i < props.Length; i++)
                {
                    var id = i;
                    var prop = props[i];
                    try
                    {
                        prop.SetValue(null, id);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                    eventNameDic[id] = prop.Name;
                }
                return eventNameDic;
            }
            else
            {
                var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                var eventNameDic = new Dictionary<int, string>();
                for (int i = 0; i < fields.Length; i++)
                {
                    var id = i;
                    var field = fields[i];
                    try
                    {
                        field.SetValue(null, id);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                    eventNameDic[id] = field.Name;
                }
                return eventNameDic;
            }
        }

        //自动填充对象字段名
        public static void InitSelfName<T>(bool property = false)
        {
            if (property)
            {
                var props = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.SetProperty);
                foreach (var prop in props)
                {
                    try
                    {
                        prop.SetValue(null, prop.Name);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
            else
            {
                var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
                foreach (var field in fields)
                {
                    try
                    {
                        field.SetValue(null, field.Name);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
        }

    }
}