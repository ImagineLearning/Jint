﻿using System;
using Jint.Native.Date;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jint.Tests.IL
{
	[TestClass]
	public class DateTests
	{
		private Jint.Engine _engine;
		[TestInitialize]
		public void Setup()
		{
			_engine = new Engine(cfg => cfg.AllowClr());
		}

		[TestMethod]
		public void TestSubtractMethod()
		{
			var code = "var now = new System.DateTime(2016, 06, 23);" +
					   "function MethodWithSubtract() {" +
					   "	var diff = now.Subtract(new System.DateTime(2016, 06, 22));" +
					   "	return diff;" +
					   "};";

			_engine.Execute(code);
			var result = _engine.Invoke("MethodWithSubtract");
			var timeSpanInstance = result.As<TimeSpanInstance>();
			Assert.IsTrue(timeSpanInstance.HasOwnProperty("TotalMilliseconds"));
			Assert.AreEqual(TimeSpan.FromDays(1).TotalMilliseconds, timeSpanInstance.GetProperty("TotalMilliseconds").Value);
		}
	}
}
