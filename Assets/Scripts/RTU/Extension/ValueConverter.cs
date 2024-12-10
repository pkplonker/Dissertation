using System;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	public static class ValueConverter
	{
		public static object ConvertValue(Type targetType, object value)
		{
			if (value == null || targetType == null)
				return null;

			if (targetType == typeof(bool))
			{
				if (value is string strValue)
				{
					return strValue == "1" || strValue.Equals("true", StringComparison.OrdinalIgnoreCase);
				}

				return Convert.ToBoolean(value);
			}

			if (targetType.IsEnum)
			{
				return Enum.Parse(targetType, value.ToString());
			}

			if (targetType != typeof(string) && targetType.IsClass)
			{
				return JsonConvert.DeserializeObject(value.ToString(), targetType);
			}
			
			if (targetType == typeof(int))
			{
				if (value is IConvertible)
				{
					long longValue = Convert.ToInt64(value);
					if (longValue == 4294967295)
					{
						return -1;
					}
				}
			}
			
			return Convert.ChangeType(value, targetType);
		}
	}
}