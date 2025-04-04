﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealTimeUpdateRuntime
{
	[Serializable]
	public class DestroyGameObjectPayload : IPayload
	{
		public static string MESSAGE_IDENTIFER = "DestroyGameObject";
		public string GameObjectName { get; set; }

		public List<string> GeneratePayload(JsonSerializerSettings JSONSettings) => new()
		{
			$"{MESSAGE_IDENTIFER}\n{JsonConvert.SerializeObject(this, Formatting.Indented, JSONSettings)}"
		};
	}
}