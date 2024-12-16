using System;
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
}