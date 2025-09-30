//*************************************************************************************
//* 作    者： 
//* 创建时间： 2025-05-06
//* 描    述：

//* ************************************************************************************
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace UFrame.BehaviourTree {
    
    public class RefUtil
    {
        private static Dictionary<Type, IEnumerable<FieldInfo>> _fieldMap = new Dictionary<Type, IEnumerable<FieldInfo>>();

        /// <summary>
        /// 反射获取所有的引用变量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetTypeRefs(Type type)
        {
            if (!_fieldMap.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(f => typeof(IRef).IsAssignableFrom(f.FieldType));
                _fieldMap[type] = fields;
            }
            return fields;
        }

    }
}

