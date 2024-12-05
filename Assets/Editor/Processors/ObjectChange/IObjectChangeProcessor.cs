using UnityEditor;

namespace RTUEditor.ObjectChange
{
	internal interface IObjectChangeProcessor
	{
		public ObjectChangeKind ChangeType { get; }
		void Process(ObjectChangeEventStream stream, int streamIx);
	}
}