using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RealTimeUpdateRuntime;
using UnityEditor;
using UnityEngine;

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

		public void Finish()
		{
			PayloadRecorderEditor.Show(this, jsonSettings);
		}
	}
}