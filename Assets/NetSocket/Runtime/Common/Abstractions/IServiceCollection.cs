

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFrame.NetSocket
{
    public interface IServiceCollection: IServiceProvider
    {
        void AddSingleton<T>(object instance);
        void AddSingleton(Type type, Func<IServiceProvider, object> createFunc = null);//packet
        void AddSingleton(object configuration);
        void AddSingleton<Key, Value>(Func<IServiceProvider, object> createFunc = null);
        void AddSingleton<T>(Func<IServiceProvider, object> createFunc = null);
        void AddSingleton(Type type1, Type type2, Func<IServiceProvider, object> createFunc = null);
    }

    public static class ServiceProviderExtend
    {
        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }
    }
}
