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
		private readonly Dictionary<Type, Type> handlers = new();

		public PropertyChangeArgsFactory()
		{
			try
			{
				handlers.Clear();

				handlers = TypeRepository.GetTypes()
					.Where(type => type.GetCustomAttribute<CustomPropertyChangeArgsAttribute>() != null)
					.ToDictionary(x => x.GetCustomAttribute<CustomPropertyChangeArgsAttribute>().Type, x => x);
			}
			catch (Exception e)
			{
				RTUDebug.LogError($"Failed to setup property change args factory {e.Message}");
			}
		}

		public IPropertyChangeArgs Get(string fullPath, string componentType, KeyValuePair<string, object> change)
		{
			var valueType = change.Value.GetType();
			if (handlers.TryGetValue(valueType, out var argsType)) { }
			else if (!handlers.TryGetValue(typeof(object), out argsType)) // default
			{
				throw new Exception($"Missing converter for property change args of type {componentType}");
			}

			var args = (IPropertyChangeArgs) Activator.CreateInstance(argsType);
			if (args == null)
			{
				throw new Exception($"Failed to property change args for {componentType}");
			}

			args.GameObjectPath = fullPath;
			args.ComponentTypeName = componentType;
			args.PropertyPath = change.Key;
			args.Value = change.Value;
			args.ValueType = valueType;
			return args;
		}
	}
}