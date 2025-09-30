using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Reflection;

namespace UFrame.ObjDrawer
{
    public class Serialized_object<T> : Serialized_object where T : class,new()
    {
        private SerializedInstance<T> _instence;
        public Serialized_object(SerializedInstance<T> instance):base(instance.Object)
        {
            this._instence = instance;
        }
        public void SetDirty()
        {
            _instence.Save();
        }
    }

    public class Serialized_object
    {
        protected Serialized_property obj_Prop;

        protected object value;

        public Serialized_object(object value)
        {
            this.value = value;
            FieldInfo info = typeof(Serialized_object).GetField("value", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
            obj_Prop = new Serialized_property(info, this);
            obj_Prop.serializedObject = this;
        }

        internal Serialized_property GetIterator()
        {
            return obj_Prop;
        }

        public Serialized_property FindProperty(string propertyPath)
        {
            return obj_Prop.FindPropertyInternal(propertyPath);
        }
    }
}