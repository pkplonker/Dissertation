using System;
using System.Collections;
using System.Collections.Generic;

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
}