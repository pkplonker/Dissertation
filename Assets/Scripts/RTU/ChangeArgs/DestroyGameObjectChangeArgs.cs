﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class DestroyGameObjectChangeArgs : IChangeArgs
	{
		public static string MESSAGE_IDENTIFER = "DestroyGameObject";
		public string ParentGameObjectPath { get; set; } = string.Empty;
		public List<string> CurrentChildren { get; set; }

		public string GeneratePayload(JsonSerializerSettings JSONSettings) =>
			$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}";
		
	}
}