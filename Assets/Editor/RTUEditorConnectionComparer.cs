using System;
using System.Collections.Generic;

namespace RTUEditor
{
	public class RTUEditorConnectionComparer : IEqualityComparer<RTUEditorConnection>
	{
		public bool Equals(RTUEditorConnection lhs, RTUEditorConnection rhs)
		{
			return StringComparer.InvariantCultureIgnoreCase
				.Equals(lhs.IPAddress, rhs.IPAddress) && lhs.Port == rhs.Port;
		}

		public int GetHashCode(RTUEditorConnection item)
		{
			return item.Port.GetHashCode() + item.IPAddress.GetHashCode();
		}
	}
}