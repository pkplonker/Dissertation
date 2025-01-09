using System.Collections.Generic;

namespace RTUEditor.AssetStore
{
	public class RendererComponentCloneStrategy : DefaultComponentCloneStratagy
	{
		protected override List<string> ExcludedProperties { get; set; } = new List<string>() {"material", "materials"};
	}
}