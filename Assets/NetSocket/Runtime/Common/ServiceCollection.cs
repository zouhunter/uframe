using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public class ServiceCollection : IServiceCollection
    {
        private Dictionary<int, Func<IServiceProvider, object>> m_createMap;
        private Dictionary<int, object> m_instanceMap;

        public ServiceCollection()
        {
            m_createMap = new Dictionary<int, Func<IServiceProvider, object>>();
            m_instanceMap = new Dictionary<int, object>();
        }

        public object GetService(Type serviceType)
        {
            var typeId = serviceType.GetHashCode();
            if (m_instanceMap.TryGetValue(typeId, out var instance))
            {
                return instance;
            }
            if (m_createMap.TryGetValue(typeId, out var createFunc))
            {
                instance = createFunc?.Invoke(this);
                m_instanceMap[typeId] = instance;
                return instance;
            }
            return null;
        }

        public Func<IServiceProvider, object> GetBuildFunc(Type type)
        {
            var contructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if(contructors.Length > 0)
            {
                var paramsNow = contructors[0].GetParameters();
                Func<IServiceProvider, object> buildFunc = new Func<IServiceProvider, object>((provider)=> {
                    var paramInstances = new object[paramsNow.Length];
                    for (int i = 0; i < paramsNow.Length; i++)
                    {
                        paramInstances[i] = GetService(paramsNow[i].ParameterType);
                    }
                    return Activator.CreateInstance(type, paramInstances);
                });
                return buildFunc;
            }
            UnityEngine.Debug.LogError("build func empty:" + type.FullName);
            return null;
        }

        public void AddSingleton(Type type, Func<IServiceProvider, object> createFunc = null)
        {
            var typeId = type.GetHashCode();
            m_createMap[typeId] = createFunc ?? GetBuildFunc(type);
        }

        public void AddSingleton(object instance)
        {
            var typeId = instance.GetType().GetHashCode();
            m_instanceMap[typeId] = instance;
        }

        public void AddSingleton<T1, T2>(Func<IServiceProvider, object> generate = null)
        {
            var typeId = typeof(T1).GetHashCode();
            m_createMap[typeId] = generate ?? GetBuildFunc(typeof(T2));
        }

        public void AddSingleton<T>(object instance)
        {
            var typeId = typeof(T).GetHashCode();
            m_instanceMap[typeId] = instance;
        }

        public void AddSingleton<T>(Func<IServiceProvider, object> generate = null)
        {
            var typeId = typeof(T).GetHashCode();
            m_createMap[typeId] = generate ?? GetBuildFunc(typeof(T));
        }

        public void AddSingleton(Type type1, Type type2, Func<IServiceProvider, object> generate = null)
        {
            var typeId = type1.GetHashCode();
            m_createMap[typeId] = generate ?? GetBuildFunc(type2);
        }
    }
}
