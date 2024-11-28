using System;
using System.Reflection;

namespace RealTimeUpdateRuntime
{
	public interface IMemberAdapter
	{
		object GetValue(object component);
		void SetValue(object component, object value);
		Type MemberType { get; }
		T GetCustomAttribute<T>() where T : Attribute;
		string Name { get; }
		MemberInfo GetMemberInfo();

	}
}