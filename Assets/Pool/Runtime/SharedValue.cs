using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFrame.Pool
{
    public class SharedValue<T> where T : new()
    {
        protected static T m_data;
        public static T sData
        {
            get
            {
                if (m_data == null)
                    m_data = new T();
                return m_data;
            }
        }
    }
}
