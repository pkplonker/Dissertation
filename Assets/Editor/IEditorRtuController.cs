using Newtonsoft.Json;

namespace RTUEditor
{
	public interface IEditorRtuController : IMessageSender
	{
		SceneGameObjectStore SceneGameObjectStore { get; }
		public JsonSerializerSettings JsonSettings { get; }
	}
}