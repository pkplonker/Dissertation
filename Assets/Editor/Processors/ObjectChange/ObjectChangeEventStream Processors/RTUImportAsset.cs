using UnityEditor;

namespace RTUEditor.ObjectChange
{
	/// <summary>
	/// Wrapper for asset imported due to the need to surpress the RTUAssetPostProcessor
	/// </summary>
	public class RTUImportAsset
	{
		public static void ImportAsset(AssetImporter importer)
		{
			using (new AssetPostProcessSurpressor())
			{
				AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
			}
		}
	}
}