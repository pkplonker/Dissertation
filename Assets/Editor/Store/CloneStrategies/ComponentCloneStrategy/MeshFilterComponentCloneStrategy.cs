using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class MeshFilterComponentCloneStrategy : DefaultComponentCloneStratagy
	{
		protected override List<string> ExcludedProperties { get; set; } = new List<string>() {"mesh"};

	}
}