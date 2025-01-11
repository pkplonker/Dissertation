using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

	public static IEnumerable<Transform> GetChildren(this Transform trans)
	{
		IList<Transform> transforms = new List<Transform>();
		foreach (var t in trans)
		{
			transforms.Add(t as Transform);
		}

		return transforms;
	}

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

	//https://discussions.unity.com/t/system-type-gettype-transform-not-work/406299/5
	public static Type GetTypeIncludingUnity(this string TypeName)
	{
		var type = Type.GetType(TypeName);

		if (type != null)
			return type;

		var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

		var assembly = Assembly.LoadWithPartialName(assemblyName);
		return assembly == null ? null : assembly.GetType(TypeName);
	}

	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			collection.Add(item);
		}
	}

	//https://stackoverflow.com/questions/51315918/how-to-encodetopng-compressed-textures-in-unity
	public static Texture2D Decompress(this Texture source)
	{
		RenderTexture renderTex = RenderTexture.GetTemporary(
			source.width,
			source.height,
			0,
			RenderTextureFormat.Default,
			RenderTextureReadWrite.Linear);

		Graphics.Blit(source, renderTex);
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = renderTex;
		Texture2D readableText = new Texture2D(source.width, source.height);
		readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		readableText.Apply();
		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary(renderTex);
		return readableText;
	}
}