using System;

namespace RealTimeUpdateRuntime
{
	[AttributeUsage(AttributeTargets.Class)]
	public class JSONCustomConverterAttribute : Attribute
	{
		public string Type;

		public JSONCustomConverterAttribute(Type type)
		{
			this.Type = type.FullName;
		}
	}
}