using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jint.Tests.Runtime
{
    /// <summary>
    /// Summary description for SamplesTest
    /// </summary>
    [TestClass]
    public class SamplesTest
    {
        private readonly Engine _engine;

        public SamplesTest()
        {
            _engine = new Engine()
                .SetValue("log", new Action<object>(x => TestContext.WriteLine(x.ToString())))
                .SetValue("assert", new Action<bool>(Assert.IsTrue))
                ;
        }

        private void RunTest(string source)
        {
            _engine.Execute(source);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GithubReadme1()
        {
            var square = new Engine()
                .SetValue("x", 3)
                .Execute("x * x")
                .GetCompletionValue()
                .ToObject();

            Assert.AreEqual(9d, square, "GithubReadme1 should be 9.0 but is " + square.ToString());
        }
    }
}
