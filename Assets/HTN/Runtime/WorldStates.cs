using System.Collections.Generic;
using System;
using System.Linq;
using UFrame.BehaviourTree;
namespace UFrame.HTN
{
    public interface IWorldStates : IVariableContent
    {
    }

    [Serializable]
    public class WorldRefStates : IWorldStates
    {
        public List<string> keys;
        public IVariableProvider variableProvider;

        /// <summary>
        /// Delegate for attribute changes.
        /// </summary>
        public delegate void ChangeEvent(string key, object value);

        /// <summary>
        /// On attribute changed.
        /// </summary>
        public ChangeEvent ValueChanged { get; set; }

        public WorldRefStates(IVariableProvider variableProvider)
        {
            this.variableProvider = variableProvider;
        }

        /// <summary>
        /// Copies a blackboard and its attributes.
        /// </summary>
        public void MakeCopy(WorldCopyStates blackboard)
        {
            CopyTo(blackboard, keys);
        }

        public void CopyTo(IVariableContent target, List<string> keys = null)
        {
            variableProvider.CopyTo(target, keys);
        }

        public T GetValue<T>(string name)
        {
            return variableProvider.GetValue<T>(name);
        }

        public bool TrySetValue(string key, object value)
        {
            var changed = variableProvider.TrySetValue(key, value);
            if (changed)
            {
                ValueChanged?.Invoke(key, value);
            }
            return changed;
        }

        public bool SetValue<T>(string key, T value)
        {
            var changed = variableProvider.SetValue(key, value);
            if (changed)
            {
                ValueChanged?.Invoke(key, value);
            }
            return changed;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            return variableProvider.TryGetValue<T>(name, out value);
        }
    }

    public class WorldCopyStates : IWorldStates
    {
        /// <summary>
        /// /// Dictionary for the key-value store.
        /// </summary>
        public Dictionary<string, object> Map { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WorldCopyStates()
        {
            Map = new Dictionary<string, object>();
        }
        /// <summary>
        /// Check if attribute store contains key.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if store contains key - otherwise false.</returns>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            return Map.ContainsKey(key);
        }
        /// <summary>
        /// Clear the key-value collection.
        /// </summary>
        public void Clear()
        {
            Map.Clear();
        }

        /// <summary>
        /// Get the value of corresponding key. Create a new
        /// instance of type if no key found.
        /// </summary>
        public object Get(Type type, string name)
        {
            object value;
            if (!Map.TryGetValue(name, out value))
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            return value;
        }

        /// <summary>
        /// Get value by key. Returns default if no key found.
        /// </summary>
        public T GetValue<T>(string key)
        {
            object value;
            if (!Map.TryGetValue(key, out value))
            {
                return default(T);
            }
            if (value == null || !(value is T))
            {
            }
            if (value is T)
            {
                T result = (T)value;
                if (true)
                {
                    return result;
                }
            }
            return default(T);
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            if (!ContainsKey(key))
                return false;
            value = GetValue<T>(key);
            return true;
        }

        /// <summary>
        /// Check if key-value collection is empty.
        /// </summary>
        public bool IsEmpty()
        {
            return Map.Count == 0;
        }

        /// <summary>
        /// Set a value by key. Notify listeners if value was changed.
        /// </summary>
        public bool SetValue<T>(string key, T value)
        {
            bool flag = false;
            if (!Map.ContainsKey(key))
            {
                flag = true;
            }
            else
            {
                object obj = Map[key];
                if (obj is T)
                {
                    T val = (T)obj;
                    if (true)
                    {
                    }
                }
                if (!object.Equals(value, obj))
                {
                    flag = true;
                }
            }
            Map[key] = value;
            return flag;
        }

        public bool TrySetValue(string key, object value)
        {
            bool flag = false;
            if (!Map.ContainsKey(key))
            {
                flag = true;
            }
            else
            {
                object obj = Map[key];
                if (!object.Equals(value, obj))
                {
                    flag = true;
                }
            }
            Map[key] = value;
            return flag;
        }

        public void CopyTo(IVariableContent target, List<string> keys = null)
        {
            foreach (var pair in Map)
            {
                if (keys != null && !keys.Contains(pair.Key))
                    continue;
                target.SetValue(pair.Key, pair.Value);
            }
        }
    }
}