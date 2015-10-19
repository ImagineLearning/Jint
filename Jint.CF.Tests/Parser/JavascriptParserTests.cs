using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Jint.Parser;
using Jint.Parser.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jint.Tests.Parser
{
    [TestClass]
    public class JavascriptParserTests
    {
        private readonly JavaScriptParser _parser = new JavaScriptParser();

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
        public void ShouldParseScriptFileJQuery()
        {
            ShouldParseScriptFileRun("jQuery.js", "1.9.1");
        }
        [TestMethod]
        public void ShouldParseScriptFileUnderscore()
        {
            ShouldParseScriptFileRun("underscore.js", "1.5.2");
        }
        [TestMethod]
        public void ShouldParseScriptFileBackbone()
        {
            ShouldParseScriptFileRun("backbone.js", "1.1.0");
        }
        [TestMethod]
        public void ShouldParseScriptFileMootools()
        {
            ShouldParseScriptFileRun("mootools.js", "1.4.5");
        }
        [TestMethod]
        public void ShouldParseScriptFileAngular()
        {
            ShouldParseScriptFileRun("angular.js", "1.2.5");
        }
        [TestMethod]
        public void ShouldParseScriptFileJXTransformer()
        {
            ShouldParseScriptFileRun("JSXTransformer.js", "0.10.0");
        }
        [TestMethod]
        public void ShouldParseScriptFileHandlebars()
        {
            ShouldParseScriptFileRun("handlebars.js", "2.0.0");
        }
        private void ShouldParseScriptFileRun(string file, string version)
        {
            const string prefix = "Jint.Tests.Parser.Scripts.";

            var assembly = Assembly.GetExecutingAssembly();
            var scriptPath = prefix + file;
            var sw = new Stopwatch();

            using (var stream = assembly.GetManifestResourceStream(scriptPath))
            {
                if (stream != null)
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var source = sr.ReadToEnd();
                        sw.Reset();
                        sw.Start();
                        var parser = new JavaScriptParser();
                        var program = parser.Parse(source);
                        Console.WriteLine("Parsed {0} {1} ({3} KB) in {2} ms", file, version, sw.ElapsedMilliseconds, (int)source.Length / 1024);
                        Assert.IsNotNull(program);
                    }
                }
            }
        }

        [TestMethod]
        public void ShouldParseThis()
        {
            var program = _parser.Parse("this");
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.AreEqual(SyntaxNodes.ThisExpression, body.First().As<ExpressionStatement>().Expression.Type);
        }

        [TestMethod]
        public void ShouldParseNull()
        {
            var program = _parser.Parse("null");
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.AreEqual(SyntaxNodes.Literal, body.First().As<ExpressionStatement>().Expression.Type);
            Assert.AreEqual(null, body.First().As<ExpressionStatement>().Expression.As<Literal>().Value);
            Assert.AreEqual("null", body.First().As<ExpressionStatement>().Expression.As<Literal>().Raw);
        }

        [TestMethod]
        public void ShouldParseNumeric()
        {
            var program = _parser.Parse(
                @"
                42
            ");
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.AreEqual(SyntaxNodes.Literal, body.First().As<ExpressionStatement>().Expression.Type);
            Assert.AreEqual(42d, body.First().As<ExpressionStatement>().Expression.As<Literal>().Value);
            Assert.AreEqual("42", body.First().As<ExpressionStatement>().Expression.As<Literal>().Raw);
        }

        [TestMethod]
        public void ShouldParseBinaryExpression()
        {
            BinaryExpression binary;

            var program = _parser.Parse("(1 + 2 ) * 3");
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.IsNotNull(binary = body.First().As<ExpressionStatement>().Expression.As<BinaryExpression>());
            Assert.AreEqual(3d, binary.Right.As<Literal>().Value);
            Assert.AreEqual(BinaryOperator.Times, binary.Operator);
            Assert.AreEqual(1d, binary.Left.As<BinaryExpression>().Left.As<Literal>().Value);
            Assert.AreEqual(2d, binary.Left.As<BinaryExpression>().Right.As<Literal>().Value);
            Assert.AreEqual(BinaryOperator.Plus, binary.Left.As<BinaryExpression>().Operator);
        }

        [TestMethod]
        public void ShouldParseNumericLiterals1()
        {
            ShouldParseNumericLiteralsRun(0, "0");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals2()
        {
            ShouldParseNumericLiteralsRun(42, "42");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals3()
        {
            ShouldParseNumericLiteralsRun(0.14, "0.14");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals4()
        {
            ShouldParseNumericLiteralsRun(3.14159, "3.14159");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals5()
        {
            ShouldParseNumericLiteralsRun(6.02214179e+23, "6.02214179e+23");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals6()
        {
            ShouldParseNumericLiteralsRun(1.492417830e-10, "1.492417830e-10");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals7()
        {
            ShouldParseNumericLiteralsRun(0, "0x0");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals8()
        {
            ShouldParseNumericLiteralsRun(0, "0x0;");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals9()
        {
            ShouldParseNumericLiteralsRun(0xabc, "0xabc");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals10()
        {
            ShouldParseNumericLiteralsRun(0xdef, "0xdef");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals11()
        {
            ShouldParseNumericLiteralsRun(0X1A, "0X1A");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals12()
        {
            ShouldParseNumericLiteralsRun(0x10, "0x10");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals13()
        {
            ShouldParseNumericLiteralsRun(0x100, "0x100");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals14()
        {
            ShouldParseNumericLiteralsRun(0X04, "0X04");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals15()
        {
            ShouldParseNumericLiteralsRun(02, "02");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals16()
        {
            ShouldParseNumericLiteralsRun(10, "012");
        }
        [TestMethod]
        public void ShouldParseNumericLiterals17()
        {
            ShouldParseNumericLiteralsRun(10, "0012");
        }
        private void ShouldParseNumericLiteralsRun(object expected, string source)
        {
            Literal literal;

            var program = _parser.Parse(source);
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.IsNotNull(literal = body.First().As<ExpressionStatement>().Expression.As<Literal>());
            Assert.AreEqual(Convert.ToDouble(expected), Convert.ToDouble(literal.Value));
        }

        [TestMethod]
        public void ShouldParseStringLiterals1()
        {
            ShouldParseStringLiteralsRun("Hello", @"'Hello'");
        }
        [TestMethod]
        public void ShouldParseStringLiterals2()
        {
            ShouldParseStringLiteralsRun("\n\r\t\v\b\f\\\'\"\0", @"'\n\r\t\v\b\f\\\'\""\0'");
        }
        [TestMethod]
        public void ShouldParseStringLiterals3()
        {
            ShouldParseStringLiteralsRun("\u0061", @"'\u0061'");
        }
        [TestMethod]
        public void ShouldParseStringLiterals4()
        {
            ShouldParseStringLiteralsRun("\x61", @"'\x61'");
        }
        [TestMethod]
        public void ShouldParseStringLiterals5()
        {
            ShouldParseStringLiteralsRun("Hello\nworld", @"'Hello\nworld'");
        }
        [TestMethod]
        public void ShouldParseStringLiterals6()
        {
            ShouldParseStringLiteralsRun("Hello\\\nworld", @"'Hello\\\nworld'");
        }
        private void ShouldParseStringLiteralsRun(string expected, string source)
        {
            Literal literal;

            var program = _parser.Parse(source);
            var body = program.Body;

            Assert.IsNotNull(body);
            Assert.AreEqual(1, body.Count());
            Assert.IsNotNull(literal = body.First().As<ExpressionStatement>().Expression.As<Literal>());
            Assert.AreEqual(expected, literal.Value);
        }

        [TestMethod]
        public void ShouldInsertSemicolons1()
        {
            ShouldInsertSemicolonsRun(@"{ x
                              ++y }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons2()
        {
            ShouldInsertSemicolonsRun(@"{ x
                              --y }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons3()
        {
            ShouldInsertSemicolonsRun(@"var x /* comment */;
                              { var x = 14, y = 3
                              z; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons4()
        {
            ShouldInsertSemicolonsRun(@"while (true) { continue
                              there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons5()
        {
            ShouldInsertSemicolonsRun(@"while (true) { continue // Comment
                              there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons6()
        {
            ShouldInsertSemicolonsRun(@"while (true) { continue /* Multiline
                              Comment */there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons7()
        {
            ShouldInsertSemicolonsRun(@"while (true) { break
                              there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons8()
        {
            ShouldInsertSemicolonsRun(@"while (true) { break // Comment
                              there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons9()
        {
            ShouldInsertSemicolonsRun(@"while (true) { break /* Multiline
                              Comment */there; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons10()
        {
            ShouldInsertSemicolonsRun(@"(function(){ return
                              x; })");
        }
        [TestMethod]
        public void ShouldInsertSemicolons11()
        {
            ShouldInsertSemicolonsRun(@"(function(){ return // Comment
                                      x; })");
        }
        [TestMethod]
        public void ShouldInsertSemicolons12()
        {
            ShouldInsertSemicolonsRun(@"(function(){ return/* Multiline
                                      Comment */x; })");
        }
        [TestMethod]
        public void ShouldInsertSemicolons13()
        {
            ShouldInsertSemicolonsRun(@"{ throw error
                                error; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons14()
        {
            ShouldInsertSemicolonsRun(@"{ throw error// Comment
                                error; }");
        }
        [TestMethod]
        public void ShouldInsertSemicolons15()
        {
            ShouldInsertSemicolonsRun(@"{ throw error/* Multiline
                              Comment */error; }");
        }
        private void ShouldInsertSemicolonsRun(string source)
        {
            var program = _parser.Parse(source);
            var body = program.Body;

            Assert.IsNotNull(body);
        }
    }
}
