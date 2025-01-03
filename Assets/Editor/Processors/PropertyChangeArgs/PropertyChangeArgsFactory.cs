using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RealTimeUpdateRuntime;

namespace RTUEditor
{
	public class PropertyChangeArgsFactory
	{
		[ItemCanBeNull]
		private readonly Dictionary<Type, Type> handlers = new();

		public PropertyChangeArgsFactory()
		{
			handlers.Clear();

			handlers = TypeRepository.GetTypes()
				.Where(type => type.GetCustomAttribute<CustomPropertyChangeArgsAttribute>() != null)
				.ToDictionary(x => x.GetCustomAttribute<CustomPropertyChangeArgsAttribute>().Type, x => x);
		}

		public IPropertyChangeArgs Get(string fullPath, string typeName, KeyValuePair<string, object> change)
		{
			var type = typeName.GetTypeIncludingUnity();
			if (handlers.TryGetValue(type, out var argsType)) { }
			else if (!handlers.TryGetValue(typeof(object), out argsType)) // default
			{
				throw new Exception($"Missing converter for property change args of type {typeName}");
			}

			var args = (IPropertyChangeArgs) Activator.CreateInstance(argsType);
			if (args == null)
			{
				throw new Exception($"Failed to property change args for {typeName}");
			}

			args.GameObjectPath = fullPath;
			args.ComponentTypeName = typeName;
			args.PropertyPath = change.Key;
			args.Value = change.Value;
			args.ValueType = change.Value.GetType();
			return args;
		}
	}
}