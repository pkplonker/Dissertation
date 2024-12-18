using System;

namespace RealTimeUpdateRuntime
{
	public static class RTUDebug
	{
#if DEBUG
        private const bool IsDebugEnabled = true;
#else
		private const bool IsDebugEnabled = false;
#endif

		public static void Log(string message)
		{
			if (IsDebugEnabled)
			{
				UnityEngine.Debug.Log(message);
			}
		}

		public static void LogWarning(string message)
		{
			if (IsDebugEnabled)
			{
				UnityEngine.Debug.LogWarning(message);
			}
		}

		public static void LogError(string message)
		{
			if (IsDebugEnabled)
			{
				UnityEngine.Debug.LogError(message);
			}
		}

		public static void LogException(Exception exception)
		{
			if (IsDebugEnabled && exception != null)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}
}