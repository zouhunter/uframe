/*-*-* Copyright (c) webxr@zht
 * Author: zouhunter
 * Creation Date: 2024-03-21
 * Version: 1.0.0
 * Description: 
 *_*/

using System;

namespace UFrame
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodePathAttribute : Attribute
    {
        public string desc;
        public NodePathAttribute(string desc)
        {
            this.desc = desc;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class PrimaryArgAttribute : Attribute
    {
        public object[] defaluts;
        public PrimaryArgAttribute(params object[] defalut)
        {
            this.defaluts = defalut;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class PrimaryConstsAttribute : Attribute
    {
        public Type type;
        public PrimaryConstsAttribute(Type type)
        {
            this.type = type;
        }
    }

}

