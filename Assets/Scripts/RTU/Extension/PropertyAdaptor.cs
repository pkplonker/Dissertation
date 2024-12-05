using System;
using System.Reflection;

namespace RealTimeUpdateRuntime
{
	public struct PropertyAdapter : IMemberAdapter
	{
		private readonly PropertyInfo property;

		public PropertyAdapter(PropertyInfo property)
		{
			this.property = property;
		}

		public object GetValue(object component)
		{
			try
			{
				return property?.GetValue(component);
			}
			catch
			{
				return null;
			}
		}

		public void SetValue(object component, object value)
		{
			property?.SetValue(component, value);
		}

		public Type MemberType => property?.PropertyType;
		public T GetCustomAttribute<T>() where T : Attribute => property?.GetCustomAttribute<T>();

		public string Name => property.Name;
		public MemberInfo GetMemberInfo() => property;
	}
}