using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UFrame.ObjDrawer
{
	internal static class EditorAssemblies
	{
		internal static List<RuntimeInitializeClassInfo> m_RuntimeInitializeClassInfoList;

		internal static int m_TotalNumRuntimeInitializeMethods;

		internal static Assembly[] loadedAssemblies
		{
			get;
			private set;
		}

		internal static IEnumerable<Type> loadedTypes
		{
			get
			{
                return null; /*EditorAssemblies.loadedAssemblies.SelectMany((Assembly assembly) => AssemblyHelper.GetTypesFromAssembly(assembly));*/
			}
		}

		internal static IEnumerable<Type> SubclassesOf(Type parent)
		{
			return from klass in EditorAssemblies.loadedTypes
			where klass.IsSubclassOf(parent)
			select klass;
		}

		
	}
}
