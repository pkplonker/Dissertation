using System.Collections.Generic;
using System.Reflection;
using RealTimeUpdateRuntime;

namespace System
{
	public static class HashCodeExtensions
	{
		private static Dictionary<Type, FieldInfo[]> cachedFields = new();

		private class ReferenceEqualityComparer : EqualityComparer<object>
		{
			public override bool Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			public override int GetHashCode(object obj)
			{
				if (obj == null) return 0;
				return obj.GetHashCode();
			}
		}

		private static bool IsPrimitive(this Type type)
		{
			if (type == typeof(String)) return true;
			return (type.IsValueType & type.IsPrimitive);
		}

		public static ulong GetStaticHashCode(this object originalObject)
		{
			ULong hash = new ULong();
			Dictionary<String, ulong> savedStringHashes = new Dictionary<string, ulong>();
			InternalCopy(originalObject, hash, savedStringHashes, new HashSet<object>(new ReferenceEqualityComparer()));

			return (ulong) hash.Value;
		}

		public static object CastToUnsigned(object number)
		{
			Type type = number.GetType();
			unchecked
			{
				if (type == typeof(int)) return (uint) (int) number;
				if (type == typeof(long)) return (ulong) (long) number;
				if (type == typeof(bool)) return Convert.ToUInt32(number);
				if (type == typeof(char)) return Convert.ToUInt32(number);
				if (type == typeof(short)) return (ushort) (short) number;
				if (type == typeof(sbyte)) return (byte) (sbyte) number;
			}

			return number;
		}

		private static ulong? DoHash(object o, ULong hash, Dictionary<string, ulong> savedHashes,
			bool hashingType = false)
		{
			if (o != null)
			{
				unchecked
				{
					ulong obj = 0;

					if (o is UnityEngine.Object unityObj)
					{
						obj = (ulong) unityObj.GetInstanceID();
					}
					else if (o is string str)
					{
						if (savedHashes.ContainsKey(str))
						{
							obj = savedHashes[str];
						}
						else
						{
							foreach (var c in str)
								obj += c;
							savedHashes[str] = obj;
						}
					}
					else if (o is IConvertible)
					{
						try
						{
							obj = Convert.ToUInt64(CastToUnsigned(o));
						}
						catch
						{
							obj = 0;
						}
					}
					else
					{
						obj = (ulong) o.GetHashCode();
					}

					hash.Value = (hash.Value * 397) ^ obj;

					if (!hashingType)
						DoHash(o.GetType().ToString(), hash, savedHashes, true);

					return hash.Value;
				}
			}

			return 0;
		}

		private class ULong
		{
			public ulong Value { get; set; }
		}

		private static void InternalCopy(object originalObject, ULong hash, Dictionary<string, ulong> savedHashes,
			HashSet<object> visited)
		{
			if (originalObject == null) return;
			if (originalObject is UnityEngine.Object unityObj)
			{
				if (unityObj != null)
				{
					DoHash(unityObj.GetInstanceID(), hash, savedHashes);
				}

				return;
			}

			var typeToReflect = originalObject.GetType();

			if (typeToReflect.IsArray)
			{
				Array array = (Array) originalObject;
				foreach (var item in array)
				{
					InternalCopy(item, hash, savedHashes, visited);
				}

				return;
			}

			if (IsPrimitive(typeToReflect))
			{
				DoHash(originalObject, hash, savedHashes);
				return;
			}

			if (visited.Contains(originalObject) || typeof(Delegate).IsAssignableFrom(typeToReflect))
			{
				return;
			}

			visited.Add(originalObject);
			CopyFields(originalObject, hash, savedHashes, visited, typeToReflect);
			RecursiveCopyBaseTypePrivateFields(originalObject, hash, savedHashes, visited, typeToReflect);
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, ULong hash,
			Dictionary<string, ulong> savedHashes, HashSet<object> visited, Type typeToReflect)
		{
			if (typeToReflect.BaseType != null)
			{
				RecursiveCopyBaseTypePrivateFields(originalObject, hash, savedHashes, visited, typeToReflect.BaseType);
				CopyFields(originalObject, hash, savedHashes, visited, typeToReflect.BaseType,
					BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
			}
		}

		private static void CopyFields(
			object originalObject,
			ULong hash,
			Dictionary<string, ulong> savedHashes,
			HashSet<object> visited,
			Type typeToReflect,
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			Func<FieldInfo, bool> filter = null)
		{
			var fields = GetFields(typeToReflect, bindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsDefined(typeof(NonSerializedAttribute), true)) continue;

				if (fieldInfo.FieldType.IsPointer || typeof(Delegate).IsAssignableFrom(fieldInfo.FieldType)) continue;

				if (filter != null && !filter(fieldInfo)) continue;

				try
				{
					var value = fieldInfo.GetValue(originalObject);

					if (IsPrimitive(fieldInfo.FieldType))
					{
						DoHash(value, hash, savedHashes);
					}
					else
					{
						InternalCopy(value, hash, savedHashes, visited);
					}
				}
				catch (Exception ex)
				{
					RTUDebug.LogWarning(
						$"Failed to get value of field {fieldInfo.Name} in type {typeToReflect.Name} : {ex.Message}");
				}
			}
		}

		private static FieldInfo[] GetFields(Type typeToReflect, BindingFlags bindingFlags)
		{
			if (cachedFields.ContainsKey(typeToReflect))
			{
				return cachedFields[typeToReflect];
			}

			var newFields = typeToReflect.GetFields(bindingFlags);
			cachedFields.Add(typeToReflect, newFields);

			return newFields;
		}

		private static void ForEach(this Array array, Action<Array, int[]> action)
		{
			if (array.LongLength == 0) return;
			ArrayTraverse walker = new ArrayTraverse(array);
			do action(array, walker.Position);
			while (walker.Step());
		}

		private class ArrayTraverse
		{
			public int[] Position;
			private int[] maxLengths;

			public ArrayTraverse(Array array)
			{
				maxLengths = new int[array.Rank];
				for (int i = 0; i < array.Rank; ++i)
				{
					maxLengths[i] = array.GetLength(i) - 1;
				}

				Position = new int[array.Rank];
			}

			public bool Step()
			{
				for (int i = 0; i < Position.Length; ++i)
				{
					if (Position[i] < maxLengths[i])
					{
						Position[i]++;
						for (int j = 0; j < i; j++)
						{
							Position[j] = 0;
						}

						return true;
					}
				}

				return false;
			}
		}
	}
}