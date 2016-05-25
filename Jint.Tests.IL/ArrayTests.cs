using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jint.Native;
using Jint.Runtime.Descriptors;

namespace Jint.Tests.IL
{
	[TestClass]
	public class ArrayTests
	{
		private Jint.Engine _engine;
		[TestInitialize]
		public void Setup()
		{
			_engine = new Engine(cfg => cfg.AllowClr());
		}

		[TestMethod]
		public void TestSelectMethod()
		{
			var code = "function TestObject(){" +
			           "	this.TestList = [1, 2, 3, 4];" +
			           "	this.CallSelect = function(){" +
					   "		return this.TestList.Select(function(x){return x*x;}).ToArray();" +
			           "	};" +
			           "};" +
			           "var myTestObject = new TestObject();" +
			           "function CallMethodWithSelect(){" +
			           "	myTestObject.TestList = myTestObject.CallSelect();" +
			           "	return myTestObject.TestList;" +
			           "};";

			_engine.Execute(code);
			var result = _engine.Invoke("CallMethodWithSelect").AsArray().GetOwnProperties().ToArray();
			Assert.AreEqual(result[0].Value.Value, 1);
			Assert.AreEqual(result[1].Value.Value, 4);
			Assert.AreEqual(result[2].Value.Value, 9);
			Assert.AreEqual(result[3].Value.Value, 16);
		}
	}
}
