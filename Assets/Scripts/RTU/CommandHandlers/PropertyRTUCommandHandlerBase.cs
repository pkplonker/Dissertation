
using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace RealTimeUpdateRuntime
{
	public abstract class PropertyRTUCommandHandlerBase : RTUCommandHandlerBase
	{
		public abstract override void Process(CommandHandlerArgs commandHandlerArgs,
			JsonSerializerSettings jsonSettings);
		public IPropertyChangeArgs ProcessInternal<T>(CommandHandlerArgs commandHandlerArgs,
			out Component component, out string fieldName, out IMemberAdapter member) where T : IPropertyChangeArgs
		{
			var args = JsonConvert.DeserializeObject<ComponentPropertyChangeArgs>(commandHandlerArgs.Payload);
			var go = GameObject.Find(args.GameObjectPath);
			var type = Type.GetType(args.ComponentTypeName);
			if (type == null)
			{
				throw new Exception("Could not determine property update type");
			}

			component = go.GetComponent(type);
			fieldName = args.PropertyPath;

			fieldName = fieldName.Trim("m_".ToCharArray());
			member = null;
			try
			{
				member = MemberAdaptorUtils.GetMemberAdapter(type, fieldName);
			}
			catch
			{
				try
				{
					member = MemberAdaptorUtils.GetMemberAdapter(type,
						new string(fieldName.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray()));
				}
				catch (Exception e)
				{
				}
			}

			return args;
		}
	}
}