using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Extensions
{
	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		if (enumerable == null)
			throw new ArgumentNullException(nameof(enumerable));

		if (action == null)
			throw new ArgumentNullException(nameof(action));

		foreach (var item in enumerable)
		{
			action(item);
		}

		return enumerable;
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) => enumerable.Where(x => x != null);

	public static HashSet<GameObject> GetAllGameObjects(this Scene scene)
	{
		// todo change to actually get all..
		return scene.GetRootGameObjects().ToHashSet();
	}

	public static IEnumerable<Transform> GetChildren(this Transform trans)
	{
		IList<Transform> transforms = new List<Transform>();
		foreach (var t in trans)
		{
			transforms.Add(t as Transform);
		}

		return transforms;
	}

	public static IEnumerable<GameObject> GetChildrenAsGameObjects(this Transform trans) =>
		trans.GetChildren().Select(x => x.gameObject);

	public static string GetFullName(this GameObject obj)
	{
		string path = "/" + obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = "/" + obj.name + path;
		}

		return path.TrimStart('/');
	}

	public static bool IsCollection(this object obj) => obj is IEnumerable && obj is not string;

	public static object CreateListFromType(this Type type) =>
		Activator.CreateInstance(typeof(List<>).MakeGenericType(type));

	public static Type GetElementTypeForCollection(this Type type)
	{
		var elementType = type.GetElementType();
		if (elementType == null)
		{
			elementType = type.GetGenericArguments()[0];
		}

		return elementType;
	}
}