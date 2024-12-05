using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RealTimeUpdateRuntime
{
	public class MemberAdaptorUtils
	{
		public static IMemberAdapter GetMemberAdapter(Type type, string fieldName)
		{
			var members = GetMemberInfo(type);
			var member = CreateMemberAdapter(members.First(x =>
				string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase)));
			return member;
		}

		private static MemberInfo[] GetMemberInfo(Type type) =>
			type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Concat(type
					.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.OfType<MemberInfo>()).ToArray();

		public static List<IMemberAdapter> GetMemberAdapters(Type type) =>
			GetMemberInfo(type).Select(x => CreateMemberAdapter(x)).ToList();

		private static IMemberAdapter CreateMemberAdapter(MemberInfo memberInfo)
		{
			return memberInfo switch
			{
				PropertyInfo prop => new PropertyAdapter(prop),
				FieldInfo field => new FieldAdapter(field),
				_ => throw new InvalidOperationException("Unsupported member type.")
			};
		}
	}
}