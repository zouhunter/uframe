using UnityEngine;
using System;
using System.Collections;

namespace UFrame.ObjDrawer
{
	[System.Serializable]
	public class SerializedInstance<T> where T: class,new() {

        public const string BASE64_IDENTIFIER = "B64|";
        [SerializeField] private string m_className;
		[SerializeField] private string m_instanceData;

		private T m_object;

		public string ClassName {
			get {
				return m_className;
			}
		}

		public T Object {
			get {
				if(m_object == null) {
					m_object = Deserialize();
				}
				return m_object;
			}
		}

		public string Data {
			get {
				return m_instanceData;
			}
		}

		public SerializedInstance(SerializedInstance<T> instance) {
			m_className = instance.m_className;
			m_instanceData = instance.m_instanceData;
		}

		public SerializedInstance(T obj) {
			UnityEngine.Assertions.Assert.IsNotNull(obj);

            m_className = obj.GetType().AssemblyQualifiedName;
			var str = JsonUtility.ToJson(obj);
			m_instanceData = EncodeString(str);
		}

		private T Deserialize() {
			Type instanceType = null;
			if(!string.IsNullOrEmpty(m_className)) {
				instanceType = Type.GetType(m_className);
			}

			if(!string.IsNullOrEmpty(m_instanceData) && instanceType != null) {
				string data = DecodeString(m_instanceData);
				T instance = new T();
				JsonUtility.FromJsonOverwrite(data, instance);
				return instance;
			}

			return default(T);
		}

		public void Save() {
			if(m_object != null) {
                m_className = m_object.GetType().AssemblyQualifiedName;
				var str = JsonUtility.ToJson(m_object);
				m_instanceData = EncodeString(str);
			}
		}

		public T Clone() {
			Save();
			return Deserialize();
		}

		public override bool Equals(object rhs)
		{
			SerializedInstance<T> other = rhs as SerializedInstance<T>; 
			if (other == null) {
				return false;
			} else {
				return other == this;
			}
		}

		public override int GetHashCode()
		{
			return (m_instanceData == null)? this.GetHashCode() : m_instanceData.GetHashCode();
		}

		public static bool operator == (SerializedInstance<T> lhs, SerializedInstance<T> rhs) {

			object lobj = lhs;
			object robj = rhs;

			if(lobj == null && robj == null) {
				return true;
			}
			if(lobj == null || robj == null) {
				return false;
			}

			return lhs.m_className != rhs.m_className && lhs.m_instanceData == rhs.m_instanceData;
		}

		public static bool operator != (SerializedInstance<T> lhs, SerializedInstance<T> rhs) {
			return !(lhs == rhs);
		}

        public static string DecodeString(string data)
        {
            if (data.StartsWith(BASE64_IDENTIFIER))
            {
                var bytes = Convert.FromBase64String(data.Substring(BASE64_IDENTIFIER.Length));
                data = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return data;
        }
        public static string EncodeString(string data)
        {
            return BASE64_IDENTIFIER +
                      Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }
    }
}

