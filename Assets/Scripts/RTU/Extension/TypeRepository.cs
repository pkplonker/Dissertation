using System;
using System.Collections.Generic;
using System.Linq;

namespace RealTimeUpdateRuntime
{
	public static class TypeRepository
	{
		private static IEnumerable<Type> types = null;

		public static IEnumerable<Type> GetTypes()
		{
			if (types == null)
			{
				types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes());
			}

			return types;
		}
	}
}