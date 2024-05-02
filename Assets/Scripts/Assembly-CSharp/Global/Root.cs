using System;
using System.Collections.Generic;
using UnityEngine;

namespace Global
{
	public class Root
	{
		private static readonly Dictionary<Type, object> Singletons = new Dictionary<Type, object>();

		public static void RegisterSingleton(object o)
		{
			RegisterSingleton(o, o.GetType());
		}

		public static void RegisterSingleton<T>(object o)
		{
			RegisterSingleton(o, typeof(T));
		}

		public static void UnregisterSingleton(object o)
		{
			UnregisterSingleton(o.GetType());
		}

		public static void UnregisterSingleton<T>()
		{
			UnregisterSingleton(typeof(T));
		}

		private static void RegisterSingleton(object o, Type type)
		{
			if (Singletons.ContainsKey(type))
			{
				Debug.LogWarning(string.Concat("Already have a singleton of type ", type, ", overwriting with new instance"));
				Singletons.Remove(type);
			}
			Singletons.Add(type, o);
		}

		private static void UnregisterSingleton(Type type)
		{
			if (!Singletons.ContainsKey(type))
			{
				Debug.LogWarning(string.Concat("Don't have a singleton of type ", type, " to remove"));
			}
			else
			{
				Singletons.Remove(type);
			}
		}

		public static T Get<T>() where T : class
		{
			return Singletons[typeof(T)] as T;
		}
	}
}
