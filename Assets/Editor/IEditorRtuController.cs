namespace RTUEditor
{
	public interface IEditorRtuController : IMessageSender
	{
		SceneGameObjectStore SceneGameObjectStore { get; set; }
	}
}