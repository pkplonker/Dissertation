using System;

namespace RealTimeUpdateRuntime
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomPropertyChangeArgsAttribute : Attribute
	{
		public Type Type;

		public CustomPropertyChangeArgsAttribute(Type type)
		{
			this.Type = type;
		}
	}
}