using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public class MemberAdaptorUtils
	{
		public static IMemberAdapter GetMemberAdapter(Type type, string fieldName)
		{
			var members = GetMemberInfo(type);
			var memberInfo = members.FirstOrDefault(x =>
				string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
			IMemberAdapter member = null;
			if (memberInfo == null)
			{
				memberInfo = members.FirstOrDefault(x =>
					string.Equals(x.Name, fieldName.Insert(0, "m_"), StringComparison.InvariantCultureIgnoreCase));
			}

			if (memberInfo == null)
			{
				throw new Exception("Member not found");
			}
			member = CreateMemberAdapter(memberInfo);

			return member;
		}

		private static MemberInfo[] GetMemberInfo(Type type) =>
			type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
				.OfType<MemberInfo>()
				.ToArray();

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