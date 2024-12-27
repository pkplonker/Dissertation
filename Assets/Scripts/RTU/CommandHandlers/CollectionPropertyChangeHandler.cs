using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealTimeUpdateRuntime
{
	public abstract class CollectionPropertyChangeHandler : PropertyRTUCommandHandlerBase
	{
		public abstract override void Process(CommandHandlerArgs commandHandlerArgs, JsonSerializerSettings jsonSettings);

		protected object CreateInstanceFromMemberInfo(IMemberAdapter memberInfo, IEnumerable<Object> elements)
		{
			var memberType = memberInfo.MemberType;

			if (memberType.IsArray)
			{
				var elementType = memberType.GetElementType();
				var arrayInstance = Array.CreateInstance(elementType, elements.Count());
				int index = 0;
				foreach (var element in elements)
				{
					arrayInstance.SetValue(Convert.ChangeType(element, elementType), index++);
				}

				return arrayInstance;
			}

			static void AddElementsToCollection(ICollection collection, IEnumerable<object> elements)
			{
				var addMethod = collection.GetType().GetMethod("Add");

				foreach (var element in elements)
				{
					addMethod?.Invoke(collection, new[] {element});
				}
			}

			if (memberType.IsGenericType)
			{
				Type genericTypeDefinition = memberType.GetGenericTypeDefinition();

				if (genericTypeDefinition == typeof(List<>))
				{
					var collectionInstance =
						Activator.CreateInstance(typeof(List<>).MakeGenericType(memberType.GenericTypeArguments));
					AddElementsToCollection((IList) collectionInstance, elements);
					return collectionInstance;
				}

				if (genericTypeDefinition == typeof(HashSet<>))
				{
					var collectionInstance =
						Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(memberType.GenericTypeArguments));
					AddElementsToCollection((ICollection) collectionInstance, elements);
					return collectionInstance;
				}
			}

			throw new InvalidOperationException($"Cannot create an instance of type {memberType}.");
		}
	}
}