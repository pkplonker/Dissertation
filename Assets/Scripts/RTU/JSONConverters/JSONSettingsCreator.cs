#if DEBUG
#define JSONDEBUG
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class JSONSettingsCreator
	{
		private readonly List<JSONCustomConverterAttribute> customConverters;

		public JSONSettingsCreator()
		{
			customConverters = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.GetCustomAttribute<JSONCustomConverterAttribute>() != null)
				.Select(type => type.GetCustomAttribute<JSONCustomConverterAttribute>())
				.ToList();
		}

		public JsonSerializerSettings Create()
		{
#if JSONDEBUG
			var settings = new JsonSerializerSettings
				{ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.None, MaxDepth = 5};
#else
			var settings = new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting
 = Formatting.Indented, MaxDepth = 5};
#endif
			foreach (var type in customConverters.Select(x => Type.GetType(x.Type)))
			{
				settings.Converters.Add((JsonConverter) Activator.CreateInstance(type));
			}

			return settings;
		}
	}
}