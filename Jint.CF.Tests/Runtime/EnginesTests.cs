using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;
using Jint.Parser.Ast;
using Jint.Parser;
using Jint.Runtime;
using Jint.Native.Number;
using System.Diagnostics;
using Jint.Runtime.Debugger;

namespace Jint.Tests.Runtime
{
    /// <summary>
    /// Summary description for EngineTests
    /// </summary>
    [TestClass]
    public class EngineTests : IDisposable
    {
        private readonly Engine _engine;
        private int countBreak = 0;
        private StepMode stepMode;

        public EngineTests()
        {
            _engine = new Engine()
                .SetValue("log", new Action<object>(x => TestContext.WriteLine(x.ToString())))
                .SetValue("assert", new Action<bool>(Assert.IsTrue))
                ;
        }

        void IDisposable.Dispose()
        {
        }

        private Engine RunTest(string source)
        {
            return _engine.Execute(source);
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
        public void ShouldInterpretScriptFile()
        {
            string file = "Scratch.js";
            const string prefix = "Jint.Tests.Runtime.Scripts.";

            var assembly = Assembly.GetExecutingAssembly();
            var scriptPath = prefix + file;

            using (var stream = assembly.GetManifestResourceStream(scriptPath))
                if (stream != null)
                    using (var sr = new StreamReader(stream))
                    {
                        var source = sr.ReadToEnd();
                        RunTest(source);
                    }
        }

        [TestMethod]
        public void ShouldInterpretLiterals1() { ShouldInterpretLiterals(42d, "42"); }
        [TestMethod]
        public void ShouldInterpretLiterals2() { ShouldInterpretLiterals("Hello", "'Hello'"); }
        public void ShouldInterpretLiterals(object expected, string source)
        {
            var engine = new Engine();
            var result = engine.Execute(source).GetCompletionValue().ToObject();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShouldInterpretVariableDeclaration()
        {
            var engine = new Engine();
            var result = engine
                .Execute("var foo = 'bar'; foo;")
                .GetCompletionValue()
                .ToObject();

            Assert.AreEqual("bar", result);
        }

        [TestMethod]
        public void ShouldInterpretBinaryExpression1() { ShouldInterpretBinaryExpressionRun(4d, "1 + 3"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression2() { ShouldInterpretBinaryExpressionRun(-2d, "1 - 3"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression3() { ShouldInterpretBinaryExpressionRun(3d, "1 * 3"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression4() { ShouldInterpretBinaryExpressionRun(2d, "6 / 3"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression5() { ShouldInterpretBinaryExpressionRun(9d, "15 & 9"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression6() { ShouldInterpretBinaryExpressionRun(15d, "15 | 9"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression7() { ShouldInterpretBinaryExpressionRun(6d, "15 ^ 9"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression8() { ShouldInterpretBinaryExpressionRun(36d, "9 << 2"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression9() { ShouldInterpretBinaryExpressionRun(2d, "9 >> 2"); }
        [TestMethod]
        public void ShouldInterpretBinaryExpression10() { ShouldInterpretBinaryExpressionRun(4d, "19 >>> 2"); }
        private void ShouldInterpretBinaryExpressionRun(object expected, string source)
        {
            var engine = new Engine();
            var result = engine.Execute(source).GetCompletionValue().ToObject();

            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void ShouldInterpretUnaryExpression1() { ShouldInterpretUnaryExpression(-59d, "~58"); }
        [TestMethod()]
        public void ShouldInterpretUnaryExpression2() { ShouldInterpretUnaryExpression(58d, "~~58"); }
        public void ShouldInterpretUnaryExpression(object expected, string source)
        {
            var engine = new Engine();
            var result = engine.Execute(source).GetCompletionValue().ToObject();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShouldEvaluateHasOwnProperty()
        {
            RunTest(@"
                        var x = {};
                        x.Bar = 42;
                        assert(x.hasOwnProperty('Bar'));
                    ");
        }

        [TestMethod]
        public void FunctionConstructorsShouldCreateNewObjects()
        {
            RunTest(@"
                        var Vehicle = function () {};
                        var vehicle = new Vehicle();
                        assert(vehicle != undefined);
                    ");
        }

        [TestMethod]
        public void NewObjectsInheritFunctionConstructorProperties()
        {
            RunTest(@"
                        var Vehicle = function () {};
                        var vehicle = new Vehicle();
                        Vehicle.prototype.wheelCount = 4;
                        assert(vehicle.wheelCount == 4);
                        assert((new Vehicle()).wheelCount == 4);
                    ");
        }

        [TestMethod]
        public void PrototypeFunctionIsInherited()
        {
            RunTest(@"
                        function Body(mass){
                           this.mass = mass;
                        }
        
                        Body.prototype.offsetMass = function(dm) {
                           this.mass += dm;
                           return this;
                        }
        
                        var b = new Body(36);
                        b.offsetMass(6);
                        assert(b.mass == 42);
                    ");

        }

        [TestMethod]
        public void FunctionConstructorCall()
        {
            RunTest(@"
                        function Body(mass){
                           this.mass = mass;
                        }
                        
                        var john = new Body(36);
                        assert(john.mass == 36);
                    ");
        }

        [TestMethod]
        public void NewObjectsShouldUsePrivateProperties()
        {
            RunTest(@"
                        var Vehicle = function (color) {
                            this.color = color;
                        };
                        var vehicle = new Vehicle('tan');
                        assert(vehicle.color == 'tan');
                    ");
        }

        [TestMethod]
        public void FunctionConstructorsShouldDefinePrototypeChain()
        {
            RunTest(@"
                        function Vehicle() {};
                        var vehicle = new Vehicle();
                        assert(vehicle.hasOwnProperty('constructor') == false);
                    ");
        }

        [TestMethod]
        public void NewObjectsConstructorIsObject()
        {
            RunTest(@"
                        var o = new Object();
                        assert(o.constructor == Object);
                    ");
        }

        [TestMethod]
        public void NewObjectsIntanceOfConstructorObject()
        {
            RunTest(@"
                        var o = new Object();
                        assert(o instanceof Object);
                    ");
        }

        [TestMethod]
        public void NewObjectsConstructorShouldBeConstructorObject()
        {
            RunTest(@"
                        var Vehicle = function () {};
                        var vehicle = new Vehicle();
                        assert(vehicle.constructor == Vehicle);
                    ");
        }

        [TestMethod]
        public void NewObjectsIntanceOfConstructorFunction()
        {
            RunTest(@"
                        var Vehicle = function () {};
                        var vehicle = new Vehicle();
                        assert(vehicle instanceof Vehicle);
                    ");
        }

        [TestMethod]
        public void ShouldEvaluateForLoops()
        {
            RunTest(@"
                        var foo = 0;
                        for (var i = 0; i < 5; i++) {
                            foo += i;
                        }
                        assert(foo == 10);
                    ");
        }

        [TestMethod]
        public void ShouldEvaluateRecursiveFunctions()
        {
            RunTest(@"
                        function fib(n) {
                            if (n < 2) {
                                return n;
                            }
                            return fib(n - 1) + fib(n - 2);
                        }
                        var result = fib(6);
                        assert(result == 8);
                    ");
        }

        [TestMethod]
        public void ShouldAccessObjectProperties()
        {
            RunTest(@"
                        var o = {};
                        o.Foo = 'bar';
                        o.Baz = 42;
                        o.Blah = o.Foo + o.Baz;
                        assert(o.Blah == 'bar42');
                    ");
        }


        [TestMethod]
        public void ShouldConstructArray()
        {
            RunTest(@"
                        var o = [];
                        assert(o.length == 0);
                    ");
        }

        [TestMethod]
        public void ArrayPushShouldIncrementLength()
        {
            RunTest(@"
                        var o = [];
                        o.push(1);
                        assert(o.length == 1);
                    ");
        }

        [TestMethod]
        public void ArrayFunctionInitializesLength()
        {
            RunTest(@"
                        assert(Array(3).length == 3);
                        assert(Array('3').length == 1);
                    ");
        }

        [TestMethod]
        public void ArrayIndexerIsAssigned()
        {
            RunTest(@"
                        var n = 8;
                        var o = Array(n);
                        for (var i = 0; i < n; i++) o[i] = i;
                        assert(o[0] == 0);
                        assert(o[7] == 7);
                    ");
        }

        [TestMethod]
        public void ArrayPopShouldDecrementLength()
        {
            RunTest(@"
                        var o = [42, 'foo'];
                        var pop = o.pop();
                        assert(o.length == 1);
                        assert(pop == 'foo');
                    ");
        }

        [TestMethod]
        public void ArrayConstructor()
        {
            RunTest(@"
                        var o = [];
                        assert(o.constructor == Array);
                    ");
        }

        [TestMethod]
        public void DateConstructor()
        {
            RunTest(@"
                        var o = new Date();
                        assert(o.constructor == Date);
                        assert(o.hasOwnProperty('constructor') == false);
                    ");
        }

        [TestMethod]
        public void ShouldConvertDateToNumber()
        {
            RunTest(@"
                        assert(Number(new Date(0)) === 0);
                    ");
        }

        [TestMethod]
        public void DatePrimitiveValueShouldBeNaN()
        {
            RunTest(@"
                        assert(isNaN(Date.prototype.valueOf()));
                    ");
        }

        [TestMethod]
        public void MathObjectIsDefined()
        {
            RunTest(@"
                        var o = Math.abs(-1)
                        assert(o == 1);
                    ");
        }

        [TestMethod]
        public void VoidShouldReturnUndefined()
        {
            RunTest(@"
                        assert(void 0 === undefined);
                        var x = '1';
                        assert(void x === undefined);
                        x = 'x'; 
                        assert (isNaN(void x) === true);
                        x = new String('-1');
                        assert (void x === undefined);
                    ");
        }

        [TestMethod]
        public void TypeofObjectShouldReturnString()
        {
            RunTest(@"
                        assert(typeof x === 'undefined');
                        assert(typeof 0 === 'number');
                        var x = 0;
                        assert (typeof x === 'number');
                        var x = new Object();
                        assert (typeof x === 'object');
                    ");
        }

        [TestMethod]
        public void MathAbsReturnsAbsolute()
        {
            RunTest(@"
                        assert(1 == Math.abs(-1));
                    ");
        }

        [TestMethod]
        public void NaNIsNan()
        {
            RunTest(@"
                        var x = NaN; 
                        assert(isNaN(NaN));
                        assert(isNaN(Math.abs(x)));
                    ");
        }

        [TestMethod]
        public void ToNumberHandlesStringObject()
        {
            RunTest(@"
                        var x = new String('1');
                        x *= undefined;
                        assert(isNaN(x));
                    ");
        }

        [TestMethod]
        public void FunctionScopesAreChained()
        {
            RunTest(@"
                        var x = 0;
        
                        function f1(){
                          function f2(){
                            return x;
                          };
                          return f2();
          
                          var x = 1;
                        }
        
                        assert(f1() === undefined);
                    ");
        }

        [TestMethod]
        public void EvalFunctionParseAndExecuteCode()
        {
            RunTest(@"
                        var x = 0;
                        eval('assert(x == 0)');
                    ");
        }

        [TestMethod]
        public void ForInStatement()
        {
            RunTest(@"
                        var x, y, str = '';
                        for(var z in this) {
                            str += z;
                        }
                        
                        assert(str == 'xystrz');
                    ");
        }

        [TestMethod]
        public void WithStatement()
        {
            RunTest(@"
                        with (Math) {
                          assert(cos(0) == 1);
                        }
                    ");
        }

        [TestMethod]
        public void ObjectExpression()
        {
            RunTest(@"
                        var o = { x: 1 };
                        assert(o.x == 1);
                    ");
        }

        [TestMethod]
        public void StringFunctionCreatesString()
        {
            RunTest(@"
                        assert(String(NaN) === 'NaN');
                    ");
        }

        [TestMethod]
        public void ScopeChainInWithStatement()
        {
            RunTest(@"
                        var x = 0;
                        var myObj = {x : 'obj'};
        
                        function f1(){
                          var x = 1;
                          function f2(){
                            with(myObj){
                              return x;
                            }
                          };
                          return f2();
                        }
        
                        assert(f1() === 'obj');
                    ");
        }

        [TestMethod]
        public void TryCatchBlockStatement()
        {
            RunTest(@"
                        var x, y, z;
                        try {
                            x = 1;
                            throw new TypeError();
                            x = 2;
                        }
                        catch(e) {
                            assert(x == 1);
                            assert(e instanceof TypeError);
                            y = 1;
                        }
                        finally {
                            assert(x == 1);
                            z = 1;
                        }
                        
                        assert(x == 1);
                        assert(y == 1);
                        assert(z == 1);
                    ");
        }

        [TestMethod]
        public void FunctionsCanBeAssigned()
        {
            RunTest(@"
                        var sin = Math.sin;
                        assert(sin(0) == 0);
                    ");
        }

        [TestMethod]
        public void FunctionArgumentsIsDefined()
        {
            RunTest(@"
                        function f() {
                            assert(arguments.length > 0);
                        }
        
                        f(42);
                    ");
        }

        [TestMethod]
        public void PrimitiveValueFunctions()
        {
            RunTest(@"
                        var s = (1).toString();
                        assert(s == '1');
                    ");
        }

        [TestMethod]
        public void OperatorsPrecedence()
        {
            object expected = true;
            string source = "'ab' == 'a' + 'b'";
            var engine = new Engine();
            var result = engine.Execute(source).GetCompletionValue().ToObject();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FunctionPrototypeShouldHaveApplyMethod()
        {
            RunTest(@"
                        var numbers = [5, 6, 2, 3, 7];
                        var max = Math.max.apply(null, numbers);
                        assert(max == 7);
                    ");
        }

        [TestMethod]
        public void ShouldEvaluateParseInt1() { ShouldEvaluateParseIntRun(double.NaN, "parseInt(NaN)"); }
        [TestMethod]
        public void ShouldEvaluateParseInt2() { ShouldEvaluateParseIntRun(double.NaN, "parseInt(null)"); }
        [TestMethod]
        public void ShouldEvaluateParseInt3() { ShouldEvaluateParseIntRun(double.NaN, "parseInt(undefined)"); }
        [TestMethod]
        public void ShouldEvaluateParseInt4() { ShouldEvaluateParseIntRun(double.NaN, "parseInt(new Boolean(true))"); }
        [TestMethod]
        public void ShouldEvaluateParseInt5() { ShouldEvaluateParseIntRun(double.NaN, "parseInt(Infinity)"); }
        [TestMethod]
        public void ShouldEvaluateParseInt6() { ShouldEvaluateParseIntRun(-1d, "parseInt(-1)"); }
        [TestMethod]
        public void ShouldEvaluateParseInt7() { ShouldEvaluateParseIntRun(-1d, "parseInt('-1')"); }
        private void ShouldEvaluateParseIntRun(object expected, string source)
        {
            var engine = new Engine();
            var result = engine.Execute(source).GetCompletionValue().ToObject();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShouldNotExecuteDebuggerStatement()
        {
            new Engine().Execute("debugger");
        }

        [TestMethod]
        public void ShouldThrowStatementCountOverflow() // TODO does not throw during unit test but it does throw in console/forms app...
        {
            try
            {
                new Engine(cfg => cfg.MaxStatements(100)).Execute("while(true);");
            }
            catch (StatementsCountOverflowException ex)
            {
                Assert.IsNotNull(ex);
            }
            //Assert.Fail();
        }

        [TestMethod]
        public void ShouldThrowTimeout()
        {
            try
            {
                new Engine(cfg => cfg.TimeoutInterval(new TimeSpan(0, 0, 0, 0, 500))).Execute("while(true);");
            }
            catch (TimeoutException ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void CanDiscardRecursion()
        {
            var script = @"var TestMethodorial = function(n) {
                if (n>1) {
                    return n * TestMethodorial(n - 1);
                }
            };

            var result = TestMethodorial(500);
            ";

            try
            {
                new Engine(cfg => cfg.LimitRecursion()).Execute(script);
            }
            catch (RecursionDepthOverflowException ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void ShouldDiscardHiddenRecursion()
        {
            var script = @"var renamedFunc;
            var exec = function(callback) {
                renamedFunc = callback;
                callback();
            };

            var result = exec(function() {
                renamedFunc();
            });
            ";

            try
            {
                new Engine(cfg => cfg.LimitRecursion()).Execute(script);
            }
            catch (RecursionDepthOverflowException ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void ShouldRecognizeAndDiscardChainedRecursion()
        {
            var script = @" var funcRoot, funcA, funcB, funcC, funcD;

            var funcRoot = function() {
                funcA();
            };
 
            var funcA = function() {
                funcB();
            };

            var funcB = function() {
                funcC();
            };

            var funcC = function() {
                funcD();
            };

            var funcD = function() {
                funcRoot();
            };

            funcRoot();
            ";

            try
            {
                new Engine(cfg => cfg.LimitRecursion()).Execute(script);
            }
            catch (RecursionDepthOverflowException ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void ShouldProvideCallChainWhenDiscardRecursion()
        {
            var script = @" var funcRoot, funcA, funcB, funcC, funcD;

            var funcRoot = function() {
                funcA();
            };
 
            var funcA = function() {
                funcB();
            };

            var funcB = function() {
                funcC();
            };

            var funcC = function() {
                funcD();
            };

            var funcD = function() {
                funcRoot();
            };

            funcRoot();
            ";

            RecursionDepthOverflowException exception = null;

            try
            {
                new Engine(cfg => cfg.LimitRecursion()).Execute(script);
            }
            catch (RecursionDepthOverflowException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("funcRoot->funcA->funcB->funcC->funcD", exception.CallChain);
            Assert.AreEqual("funcRoot", exception.CallExpressionReference);
        }

        [TestMethod]
        public void ShouldAllowShallowRecursion()
        {
            var script = @"var TestMethodorial = function(n) {
                if (n>1) {
                    return n * TestMethodorial(n - 1);
                }
            };

            var result = TestMethodorial(8);
            ";

            new Engine(cfg => cfg.LimitRecursion(20)).Execute(script);
        }

        [TestMethod]
        public void ShouldDiscardDeepRecursion()
        {
            var script = @"var TestMethodorial = function(n) {
                if (n>1) {
                    return n * TestMethodorial(n - 1);
                }
            };

            var result = TestMethodorial(38);
            ";

            try
            {
                new Engine(cfg => cfg.LimitRecursion(20)).Execute(script);
            }
            catch (RecursionDepthOverflowException ex)
            {
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void ShouldConvertDoubleToStringWithoutLosingPrecision()
        {
            RunTest(@"
                        assert(String(14.915832707045631) === '14.915832707045631');
                        assert(String(-14.915832707045631) === '-14.915832707045631');
                        assert(String(0.5) === '0.5');
                        assert(String(0.00000001) === '1e-8');
                        assert(String(0.000001) === '0.000001');
                        assert(String(-1.0) === '-1');
                        assert(String(30.0) === '30');
                        assert(String(0.2388906159889881) === '0.2388906159889881');
                    ");
        }

        [TestMethod]
        public void ShouldWriteNumbersUsingBases()
        {
            RunTest(@"
                        assert(15.0.toString() === '15');
                        assert(15.0.toString(2) === '1111');
                        assert(15.0.toString(8) === '17');
                        assert(15.0.toString(16) === 'f');
                        assert(15.0.toString(17) === 'f');
                        assert(15.0.toString(36) === 'f');
                        assert(15.1.toString(36) === 'f.3llllllllkau6snqkpygsc3di');
                    ");
        }

        [TestMethod]
        public void ShouldNotAlterSlashesInRegex()
        {
            RunTest(@"
                        assert(new RegExp('/').toString() === '///');
                    ");
        }

        [TestMethod]
        public void ShouldHandleEscapedSlashesInRegex()
        {
            RunTest(@"
                        var regex = /[a-z]\/[a-z]/;
                        assert(regex.test('a/b') === true);
                        assert(regex.test('a\\/b') === false);
                    ");
        }

        [TestMethod]
        public void ShouldComputeFractionInBase()
        {
            Assert.AreEqual("011", NumberPrototype.ToFractionBase(0.375, 2));
            Assert.AreEqual("14141414141414141414141414141414141414141414141414", NumberPrototype.ToFractionBase(0.375, 5));
        }

        [TestMethod]
        public void ShouldInvokeAFunctionValue()
        {
            RunTest(@"
                        function add(x, y) { return x + y; }
                    ");

            var add = _engine.GetValue("add");

            Assert.AreEqual(3, add.Invoke(1, 2));
        }

        [TestMethod]
        public void ShouldNotInvokeNonFunctionValue()
        {
            RunTest(@"
                        var x= 10;
                    ");

            var x = _engine.GetValue("x");

            try
            {
                TestContext.WriteLine("x = " + x);
                x.Invoke(1, 2);

                Assert.Fail("Test should throw ArgumentException");
            }
            catch (ArgumentException aex)
            {
                Assert.IsNotNull(aex);
            }
            catch (Exception ex)
            {
                Assert.Fail("Wrong Exception - " + ex.ToString());
            }
        }

        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun1() { ShouldConvertNumbersToDifferentBaseRun("0", 0, 16); }
        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun2() { ShouldConvertNumbersToDifferentBaseRun("1", 1, 16); }
        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun3() { ShouldConvertNumbersToDifferentBaseRun("100", 100, 10); }
        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun4() { ShouldConvertNumbersToDifferentBaseRun("1100100", 100, 2); }
        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun5() { ShouldConvertNumbersToDifferentBaseRun("2s", 100, 36); }
        [TestMethod]
        public void ShouldConvertNumbersToDifferentBaseRun6() { ShouldConvertNumbersToDifferentBaseRun("2qgpckvng1s", 10000000000000000L, 36); }
        private void ShouldConvertNumbersToDifferentBaseRun(string expected, long number, int radix)
        {
            var result = NumberPrototype.ToBase(number, radix);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void JsonParserShouldParseNegativeNumber()
        {
            RunTest(@"
                var a = JSON.parse('{ ""x"":-1 }');
                assert(a.x === -1);

                var b = JSON.parse('{ ""x"": -1 }');
                assert(b.x === -1);
            ");
        }

        [TestMethod]
        public void JsonParserShouldDetectInvalidNegativeNumberSyntax()
        {
            RunTest(@"
                try {
                    JSON.parse('{ ""x"": -.1 }'); // Not allowed
                    assert(false);
                }
                catch(ex) {
                    assert(ex instanceof SyntaxError);
                }
            ");

            RunTest(@"
                try {
                    JSON.parse('{ ""x"": - 1 }'); // Not allowed
                    assert(false);
                }
                catch(ex) {
                    assert(ex instanceof SyntaxError);
                }
            ");
        }

        [TestMethod]
        public void ShouldBeCultureInvariant() // TODO FS: CF has no CurrentCulture / Disable / Remove?
        {
            //// decimals in french are separated by commas
            //Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var engine = new Engine();

            var result = engine.Execute("1.2 + 2.1").GetCompletionValue().AsNumber();
            Assert.AreEqual(3.3d, result);

            result = engine.Execute("JSON.parse('{\"x\" : 3.3}').x").GetCompletionValue().AsNumber();
            Assert.AreEqual(3.3d, result);
        }

        [TestMethod]
        public void JsonParseAndStringify() // TODO FS: eigenlijk al in test boven....
        {
            var engine = new Engine();

            var result = engine.Execute("JSON.stringify(JSON.parse('{\"x\" : 3.3}'))").GetCompletionValue();
            Assert.AreEqual("{\"x\":3.3}", result.ToString());
        }

        [TestMethod]
        public void ShouldGetTheLastSyntaxNode()
        {
            var engine = new Engine();
            engine.Execute("1.2");

            var result = engine.GetLastSyntaxNode();
            Assert.AreEqual(SyntaxNodes.Literal, result.Type);
        }

        [TestMethod]
        public void ShouldGetParseErrorLocation()
        {
            var engine = new Engine();
            try
            {
                engine.Execute("1.2+ new", new ParserOptions { Source = "jQuery.js" });
            }
            catch (ParserException e)
            {
                TestContext.WriteLine("LineNumber: " + e.LineNumber.ToString());
                TestContext.WriteLine("Column: " + e.Column.ToString());
                TestContext.WriteLine("Source: " + e.Source);
                Assert.AreEqual(1, e.LineNumber);
                Assert.AreEqual(9, e.Column);
                Assert.AreEqual("jQuery.js", e.Source);
            }
        }

        [TestMethod]
        public void ParseShouldReturnNumber()
        {
            var engine = new Engine();

            var result = engine.Execute("Date.parse('1970-01-01');").GetCompletionValue().AsNumber();
            Assert.AreEqual(0, result);
        }

#if !__CF__
        [TestMethod]
        public void UtcShouldUseUtc()
        {
            const string customName = "Custom Time";
            var customTimeZone = TimeZoneInfo.CreateCustomTimeZone(customName, new TimeSpan(7, 11, 0), customName, customName, customName, null, false);
            var engine = new Engine(cfg => cfg.LocalTimeZone(customTimeZone));

            var result = engine.Execute("Date.UTC(1970,0,1)").GetCompletionValue().AsNumber();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ShouldUseLocalTimeZoneOverride()
        {
            const string customName = "Custom Time";
            var customTimeZone = TimeZoneInfo.CreateCustomTimeZone(customName, new TimeSpan(0, 11, 0), customName, customName, customName, null, false);

            var engine = new Engine(cfg => cfg.LocalTimeZone(customTimeZone));

            var epochGetLocalMinutes = engine.Execute("var d = new Date(0); d.getMinutes();").GetCompletionValue().AsNumber();
            Assert.AreEqual(11, epochGetLocalMinutes);

            var localEpochGetUtcMinutes = engine.Execute("var d = new Date(1970,0,1); d.getUTCMinutes();").GetCompletionValue().AsNumber();
            Assert.AreEqual(-11, localEpochGetUtcMinutes);

            var parseLocalEpoch = engine.Execute("Date.parse('January 1, 1970');").GetCompletionValue().AsNumber();
            Assert.AreEqual(-11 * 60 * 1000, parseLocalEpoch);

            var epochToLocalString = engine.Execute("var d = new Date(0); d.toString();").GetCompletionValue().AsString();
            Assert.AreEqual("Thu Jan 01 1970 00:11:00 GMT", epochToLocalString);
        }
#endif

        [TestMethod]
        public void ShouldParseAsUtc1() { ShouldParseAsUtc("1970"); }
        [TestMethod]
        public void ShouldParseAsUtc2() { ShouldParseAsUtc("1970-01"); }
        [TestMethod]
        public void ShouldParseAsUtc3() { ShouldParseAsUtc("1970-01-01"); }
        [TestMethod]
        public void ShouldParseAsUtc4() { ShouldParseAsUtc("1970-01-01T00:00"); }
        [TestMethod]
        public void ShouldParseAsUtc5() { ShouldParseAsUtc("1970-01-01T00:00:00"); }
        [TestMethod]
        public void ShouldParseAsUtc6() { ShouldParseAsUtc("1970-01-01T00:00:00.000"); }
        [TestMethod]
        public void ShouldParseAsUtc7() { ShouldParseAsUtc("1970Z"); }
        [TestMethod]
        public void ShouldParseAsUtc8() { ShouldParseAsUtc("1970-1Z"); }
        [TestMethod]
        public void ShouldParseAsUtc9() { ShouldParseAsUtc("1970-1-1Z"); }
        [TestMethod]
        public void ShouldParseAsUtc10() { ShouldParseAsUtc("1970-1-1T0:0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc11() { ShouldParseAsUtc("1970-1-1T0:0:0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc12() { ShouldParseAsUtc("1970-1-1T0:0:0.0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc13() { ShouldParseAsUtc("1970/1Z"); }
        [TestMethod]
        public void ShouldParseAsUtc14() { ShouldParseAsUtc("1970/1/1Z"); }
        [TestMethod]
        public void ShouldParseAsUtc15() { ShouldParseAsUtc("1970/1/1 0:0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc16() { ShouldParseAsUtc("1970/1/1 0:0:0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc17() { ShouldParseAsUtc("1970/1/1 0:0:0.0Z"); }
        [TestMethod]
        public void ShouldParseAsUtc18() { ShouldParseAsUtc("January 1, 1970 GMT"); }
        [TestMethod]
        public void ShouldParseAsUtc19() { ShouldParseAsUtc("1970-01-01T00:00:00.000-00:00"); }
        public void ShouldParseAsUtc(string date)   // CompactFramework NotImplemented: TimeZoneInfo not supported in CF.
        {
            //const string customName = "Custom Time";
            //var customTimeZone = TimeZoneInfo.CreateCustomTimeZone(customName, new TimeSpan(7, 11, 0), customName, customName, customName, null, false);
            //var engine = new Engine(cfg => cfg.LocalTimeZone(customTimeZone));
            var engine = new Engine();

            engine.SetValue("d", date);
            var result = engine.Execute("Date.parse(d);").GetCompletionValue().AsNumber();

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ShouldParseAsLocalTime1() { ShouldParseAsLocalTime("1970/01"); }
        [TestMethod]
        public void ShouldParseAsLocalTime2() { ShouldParseAsLocalTime("1970/01/01"); }
        [TestMethod]
        public void ShouldParseAsLocalTime3() { ShouldParseAsLocalTime("1970/01/01T00:00"); }
        [TestMethod]
        public void ShouldParseAsLocalTime4() { ShouldParseAsLocalTime("1970/01/01 00:00"); }
        [TestMethod]
        public void ShouldParseAsLocalTime5() { ShouldParseAsLocalTime("1970-1"); }
        [TestMethod]
        public void ShouldParseAsLocalTime6() { ShouldParseAsLocalTime("1970-1-1"); }
        [TestMethod]
        public void ShouldParseAsLocalTime7() { ShouldParseAsLocalTime("1970-1-1T0:0"); }
        [TestMethod]
        public void ShouldParseAsLocalTime8() { ShouldParseAsLocalTime("1970-1-1 0:0"); }
        [TestMethod]
        public void ShouldParseAsLocalTime9() { ShouldParseAsLocalTime("1970/1"); }
        [TestMethod]
        public void ShouldParseAsLocalTime10() { ShouldParseAsLocalTime("1970/1/1"); }
        [TestMethod]
        public void ShouldParseAsLocalTime11() { ShouldParseAsLocalTime("1970/1/1T0:0"); }
        [TestMethod]
        public void ShouldParseAsLocalTime12() { ShouldParseAsLocalTime("1970/1/1 0:0"); }
        [TestMethod]
        public void ShouldParseAsLocalTime13() { ShouldParseAsLocalTime("01-1970"); }
        [TestMethod]
        public void ShouldParseAsLocalTime14() { ShouldParseAsLocalTime("01-01-1970"); }
        [TestMethod]
        public void ShouldParseAsLocalTime15() { ShouldParseAsLocalTime("January 1, 1970"); }
        [TestMethod]
        public void ShouldParseAsLocalTime16() { ShouldParseAsLocalTime("1970-01-01T00:00:00.000+00:11"); }
        public void ShouldParseAsLocalTime(string date)   // CompactFramework NotImplemented: TimeZoneInfo not supported in CF.
        {
            //const string customName = "Custom Time";
            //var customTimeZone = TimeZoneInfo.CreateCustomTimeZone(customName, new TimeSpan(0, 11, 0), customName, customName, customName, null, false);
            //var engine = new Engine(cfg => cfg.LocalTimeZone(customTimeZone)).SetValue("d", date);
            var engine = new Engine().SetValue("d", date);

            var result = engine.Execute("Date.parse(d);").GetCompletionValue().AsNumber();

            Assert.AreEqual(7 * 1000 * 60 * 60, result);
        }

        [TestMethod]
        public void EmptyStringShouldMatchRegex()
        {
            RunTest(@"
                var regex = /^(?:$)/g;
                assert(''.match(regex) instanceof Array);
            ");
        }

#if !__CF__ // No WebClient class in CF. Besides similar test is already available in the JavascriptParserTests class.
        [TestMethod]
        public void ShouldExecuteHandlebars()
        {
            var url = "http://cdnjs.cloudflare.com/ajax/libs/handlebars.js/2.0.0/handlebars.js";
            var content = new WebClient().DownloadString(url);

            RunTest(content);

            RunTest(@"
                var source = 'Hello {{name}}';
                var template = Handlebars.compile(source);
                var context = {name: 'Paul'};
                var html = template(context);

                assert('Hello Paul' == html);
            ");
        }
#endif

        [TestMethod]
        public void DateParseReturnsNaN()
        {
            RunTest(@"
                var d = Date.parse('not a date');
                assert(isNaN(d));
            ");
        }

        [TestMethod]
        public void ShouldIgnoreHtmlComments()
        {
            RunTest(@"
                var d = Date.parse('not a date'); <!-- a comment -->
                assert(isNaN(d));
            ");
        }

        [TestMethod]
        public void DateShouldAllowEntireDotNetDateRange()
        {
            var engine = new Engine();

            var minValue = engine.Execute("new Date('0001-01-01T00:00:00.000')").GetCompletionValue().ToObject();
            Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc), minValue);

            var maxValue = engine.Execute("new Date('9999-12-31T23:59:59.999')").GetCompletionValue().ToObject();
            Assert.AreEqual(new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc), maxValue);
        }

        [TestMethod]
        public void ShouldConstructNewArrayWithInteger()
        {
            RunTest(@"
                var a = new Array(3);
                assert(a.length === 3);
                assert(a[0] == undefined);
                assert(a[1] == undefined);
                assert(a[2] == undefined);
            ");
        }

        [TestMethod]
        public void ShouldConstructNewArrayWithString()
        {
            RunTest(@"
                var a = new Array('foo');
                assert(a.length === 1);
                assert(a[0] === 'foo');
            ");
        }

        [TestMethod]
        public void ShouldThrowRangeExceptionWhenConstructedWithNonInteger()
        {
            RunTest(@"
                var result = false;
                try {
                    var a = new Array(3.4);
                }
                catch(e) {
                    result = e instanceof RangeError;
                }

                assert(result);                
            ");
        }

        [TestMethod]
        public void ShouldInitializeArrayWithSingleIngegerValue()
        {
            RunTest(@"
                var a = [3];
                assert(a.length === 1);
                assert(a[0] === 3);
            ");
        }

        [TestMethod]
        public void ShouldInitializeJsonObjectArrayWithSingleIntegerValue()
        {
            RunTest(@"
                var x = JSON.parse('{ ""a"": [3] }');
                assert(x.a.length === 1);
                assert(x.a[0] === 3);
            ");
        }

        [TestMethod]
        public void ShouldInitializeJsonArrayWithSingleIntegerValue()
        {
            RunTest(@"
                var a = JSON.parse('[3]');
                assert(a.length === 1);
                assert(a[0] === 3);
            ");
        }

        [TestMethod]
        public void ShouldReturnTrueForEmptyIsNaNStatement()
        {
            RunTest(@"
                assert(true === isNaN());
            ");
        }

        [TestMethod]
        public void ShouldRoundToFixedDecimal1() { ShouldRoundToFixedDecimal(4d, 0, "4"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal2() { ShouldRoundToFixedDecimal(4d, 1, "4.0"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal3() { ShouldRoundToFixedDecimal(4d, 2, "4.00"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal4() { ShouldRoundToFixedDecimal(28.995, 2, "29.00"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal5() { ShouldRoundToFixedDecimal(-28.995, 2, "-29.00"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal6() { ShouldRoundToFixedDecimal(-28.495, 2, "-28.50"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal7() { ShouldRoundToFixedDecimal(-28.445, 2, "-28.45"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal8() { ShouldRoundToFixedDecimal(28.445, 2, "28.45"); }
        [TestMethod]
        public void ShouldRoundToFixedDecimal9() { ShouldRoundToFixedDecimal(10.995, 0, "11"); }
        public void ShouldRoundToFixedDecimal(double number, int fractionDigits, string result)
        {
            var engine = new Engine();
            var value = engine.Execute(
                String.Format("new Number({0}).toFixed({1})",
                    number.ToString(CultureInfo.InvariantCulture),
                    fractionDigits.ToString(CultureInfo.InvariantCulture)))
                .GetCompletionValue().ToObject();

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void ShouldSortArrayWhenCompareFunctionReturnsFloatingPointNumber()
        {
            RunTest(@"
                var nums = [1, 1.1, 1.2, 2, 2, 2.1, 2.2];
                nums.sort(function(a,b){return b-a;});
                assert(nums[0] === 2.2);
                assert(nums[1] === 2.1);
                assert(nums[2] === 2);
                assert(nums[3] === 2);
                assert(nums[4] === 1.2);
                assert(nums[5] === 1.1);
                assert(nums[6] === 1);
            ");
        }

        [TestMethod]
        public void ShouldBreakWhenBreakpointIsReached()
        {
            countBreak = 0;
            stepMode = StepMode.None;

            var engine = new Engine(options => options.DebugMode());

            engine.Break += EngineStep;

            engine.BreakPoints.Add(new BreakPoint(1, 1));

            engine.Execute(@"var local = true;
                if (local === true)
                {}");

            engine.Break -= EngineStep;

            Assert.AreEqual(1, countBreak);
        }

        [TestMethod]
        public void ShouldExecuteStepByStep()
        {
            countBreak = 0;
            stepMode = StepMode.Into;

            var engine = new Engine(options => options.DebugMode());

            engine.Step += EngineStep;

            engine.Execute(@"var local = true;
                var creatingSomeOtherLine = 0;
                var lastOneIPromise = true");

            engine.Step -= EngineStep;

            Assert.AreEqual(3, countBreak);
        }

        [TestMethod]
        public void ShouldNotBreakTwiceIfSteppingOverBreakpoint()
        {
            countBreak = 0;
            stepMode = StepMode.Into;

            var engine = new Engine(options => options.DebugMode());
            engine.BreakPoints.Add(new BreakPoint(1, 1));
            engine.Step += EngineStep;
            engine.Break += EngineStep;

            engine.Execute(@"var local = true;");

            engine.Step -= EngineStep;
            engine.Break -= EngineStep;

            Assert.AreEqual(1, countBreak);
        }

        private StepMode EngineStep(object sender, DebugInformation debugInfo)
        {
            Assert.IsNotNull(sender);
            Assert.IsInstanceOfType(sender, typeof(Engine));
            Assert.IsNotNull(debugInfo);

            countBreak++;
            return stepMode;
        }

        [TestMethod]
        public void ShouldShowProperDebugInformation()
        {
            countBreak = 0;
            stepMode = StepMode.None;

            var engine = new Engine(options => options.DebugMode());
            engine.BreakPoints.Add(new BreakPoint(5, 0));
            engine.Break += EngineStepVerifyDebugInfo;

            engine.Execute(@"var global = true;
                            function func1()
                            {
                                var local = false;
;
                            }
                            func1();");

            engine.Break -= EngineStepVerifyDebugInfo;

            Assert.AreEqual(1, countBreak);
        }

        private StepMode EngineStepVerifyDebugInfo(object sender, DebugInformation debugInfo)
        {
            Assert.IsNotNull(sender);
            Assert.IsInstanceOfType(sender, typeof(Engine));
            Assert.IsNotNull(debugInfo);

            Assert.IsNotNull(debugInfo.CallStack);
            Assert.IsNotNull(debugInfo.CurrentStatement);
            Assert.IsNotNull(debugInfo.Locals);

            Assert.AreEqual(1, debugInfo.CallStack.Count);
            Assert.AreEqual("func1()", debugInfo.CallStack.Peek());
            Assert.IsTrue(debugInfo.Globals.Where(kvp => kvp.Key.Equals("global", StringComparison.Ordinal) && kvp.Value.AsBoolean() == true).Any());
            Assert.IsTrue(debugInfo.Globals.Where(kvp => kvp.Key.Equals("local", StringComparison.Ordinal) && kvp.Value.AsBoolean() == false).Any());
            Assert.IsTrue(debugInfo.Locals.Where(kvp => kvp.Key.Equals("local", StringComparison.Ordinal) && kvp.Value.AsBoolean() == false).Any());
            Assert.IsFalse(debugInfo.Locals.Where(kvp => kvp.Key.Equals("global", StringComparison.Ordinal)).Any());

            countBreak++;
            return stepMode;
        }

        [TestMethod]
        public void ShouldBreakWhenConditionIsMatched()
        {
            countBreak = 0;
            stepMode = StepMode.None;

            var engine = new Engine(options => options.DebugMode());

            engine.Break += EngineStep;

            engine.BreakPoints.Add(new BreakPoint(5, 16, "condition === true"));
            engine.BreakPoints.Add(new BreakPoint(6, 16, "condition === false"));

            engine.Execute(@"var local = true;
                var condition = true;
                if (local === true)
                {
                ;
                ;
                }");

            engine.Break -= EngineStep;

            Assert.AreEqual(1, countBreak);
        }

        [TestMethod]
        public void ShouldNotStepInSameLevelStatementsWhenStepOut()
        {
            countBreak = 0;
            stepMode = StepMode.Out;

            var engine = new Engine(options => options.DebugMode());

            engine.Step += EngineStep;

            engine.Execute(@"function func() // first step - then stepping out
                {
                    ; // shall not step
                    ; // not even here
                }
                func(); // shall not step                 
                ; // shall not step ");

            engine.Step -= EngineStep;

            Assert.AreEqual(1, countBreak);
        }

        [TestMethod]
        public void ShouldNotStepInIfRequiredToStepOut()
        {
            countBreak = 0;

            var engine = new Engine(options => options.DebugMode());

            engine.Step += EngineStepOutWhenInsideFunction;

            engine.Execute(@"function func() // first step
                {
                    ; // third step - now stepping out
                    ; // it should not step here
                }
                func(); // second step                 
                ; // fourth step ");

            engine.Step -= EngineStepOutWhenInsideFunction;

            Assert.AreEqual(4, countBreak);
        }

        private StepMode EngineStepOutWhenInsideFunction(object sender, DebugInformation debugInfo)
        {
            Assert.IsNotNull(sender);
            Assert.IsInstanceOfType(sender, typeof(Engine));
            Assert.IsNotNull(debugInfo);

            countBreak++;
            if (debugInfo.CallStack.Count > 0)
                return StepMode.Out;

            return StepMode.Into;
        }

        [TestMethod]
        public void ShouldBreakWhenStatementIsMultiLine()
        {
            countBreak = 0;
            stepMode = StepMode.None;

            var engine = new Engine(options => options.DebugMode());
            engine.BreakPoints.Add(new BreakPoint(4, 33));
            engine.Break += EngineStep;

            engine.Execute(@"var global = true;
                            function func1()
                            {
                                var local = 
                                    false;
                            }
                            func1();");

            engine.Break -= EngineStep;

            Assert.AreEqual(1, countBreak);
        }

        [TestMethod]
        public void ShouldNotStepInsideIfRequiredToStepOver()
        {
            countBreak = 0;

            var engine = new Engine(options => options.DebugMode());

            stepMode = StepMode.Over;
            engine.Step += EngineStep;

            engine.Execute(@"function func() // first step
                {
                    ; // third step - it shall not step here
                    ; // it shall not step here
                }
                func(); // second step                 
                ; // third step ");

            engine.Step -= EngineStep;

            Assert.AreEqual(3, countBreak);
        }

        [TestMethod]
        public void ShouldStepAllStatementsWithoutInvocationsIfStepOver()
        {
            countBreak = 0;

            var engine = new Engine(options => options.DebugMode());

            stepMode = StepMode.Over;
            engine.Step += EngineStep;

            engine.Execute(@"var step1 = 1; // first step
                var step2 = 2; // second step                 
                if (step1 !== step2) // third step
                { // fourth step
                    ; // fifth step
                }");

            engine.Step -= EngineStep;

            Assert.AreEqual(5, countBreak);
        }

        [TestMethod]
        public void ShouldEvaluateVariableAssignmentFromLeftToRight()
        {
            RunTest(@"
                var keys = ['a']
                  , source = { a: 3}
                  , target = {}
                  , key
                  , i = 0;
                target[key = keys[i++]] = source[key];
                assert(i == 1);
                assert(key == 'a');
                assert(target[key] == 3);
            ");
        }

        [TestMethod]
        public void ObjectShouldBeExtensible()
        {
            RunTest(@"
                try {
                    Object.defineProperty(Object.defineProperty, 'foo', { value: 1 });
                }
                catch(e) {
                    assert(false);
                }
            ");
        }

        [TestMethod]
        public void ArrayIndexShouldBeConvertedToUint32()
        {
            // This is missing from ECMA tests suite
            // http://www.ecma-international.org/ecma-262/5.1/#sec-15.4

            RunTest(@"
                var a = [ 'foo' ];
                assert(a[0] === 'foo');
                assert(a['0'] === 'foo');
                assert(a['00'] === undefined);
            ");
        }

        [TestMethod]
        public void DatePrototypeFunctionWorkOnDateOnly()
        {
            RunTest(@"
                try {
                    var myObj = Object.create(Date.prototype);
                    myObj.toDateString();
                } catch (e) {
                    assert(e instanceof TypeError);
                }
            ");
        }
    }
}
