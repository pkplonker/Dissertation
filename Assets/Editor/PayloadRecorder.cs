using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;

namespace RTUEditor
{
	public class PayloadRecorder
	{
		public List<IPayload> Payloads { get; private set; }= new();
		private readonly JsonSerializerSettings jsonSettings;

		public PayloadRecorder(JsonSerializerSettings settings)
		{
			this.jsonSettings = settings;
		}

		public void Start()
		{
			Payloads.Clear();
			PayloadRecorderEditor.Finish();
		}

		public void Record(IPayload payload)
		{
			Payloads.Add(payload);
		}

		public void Finish(Action<bool> finishCompleteCallback)
		{
			PayloadRecorderEditor.Show(this, jsonSettings,finishCompleteCallback);
		}
	}
}