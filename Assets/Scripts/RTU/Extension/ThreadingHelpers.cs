﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealTimeUpdateRuntime
{
	public class ThreadingHelpers
	{
		public static T FunctionOnScheduler<T>(Func<T> action, TaskScheduler scheduler) =>
			Task.Factory.StartNew(action, new CancellationToken(),
				new TaskCreationOptions(), scheduler).Result;

		public static  void ActionOnScheduler(Action action, TaskScheduler scheduler) =>
			Task.Factory.StartNew(action, new CancellationToken(),
				new TaskCreationOptions(), scheduler).Wait();
	}
}