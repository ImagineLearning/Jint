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
using Shapes;
using Jint.Tests.Runtime.Domain;
using Jint.Native;
using Jint.Native.Object;
using Jint.Tests.Runtime.Converters;

namespace Jint.Tests.Runtime
{
    /// <summary>
    /// Summary description for InteropTests
    /// </summary>
    [TestClass]
    public class InteropTests : IDisposable
    {
        private readonly Engine _engine;

        public InteropTests()
        {
            _engine = new Engine(cfg => cfg.AllowClr(typeof(Shape).Assembly, typeof(System.Windows.Forms.Button).Assembly, typeof(Jint.Engine).Assembly))
                .SetValue("log", new Action<object>(x => TestContext.WriteLine(x.ToString())))
                .SetValue("assert", new Action<bool>(Assert.IsTrue))
                ;
        }

        void IDisposable.Dispose()
        {
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
        public void PrimitiveTypesCanBeSet()
        {
            _engine.SetValue("x", 10);
            _engine.SetValue("y", true);
            _engine.SetValue("z", "foo");

            RunTest(@"
                assert(x === 10);
                assert(y === true);
                assert(z === 'foo');
            ");
        }

        [TestMethod]
        public void DelegatesCanBeSet()
        {
            _engine.SetValue("square", new Func<double, double>(x => x * x));

            RunTest(@"
                assert(square(10) === 100);
            ");
        }

        [TestMethod]
        public void DelegateWithNullableParameterCanBePassedANull()
        {
            _engine.SetValue("isnull", new Func<double?, bool>(x => x == null));

            RunTest(@"
                assert(isnull(null) === true);
            ");
        }

        [TestMethod]
        public void DelegateWithObjectParameterCanBePassedANull()
        {
            _engine.SetValue("isnull", new Func<object, bool>(x => x == null));

            RunTest(@"
                assert(isnull(null) === true);
            ");
        }

        [TestMethod]
        public void DelegateWithNullableParameterCanBePassedAnUndefined()
        {
            _engine.SetValue("isnull", new Func<double?, bool>(x => x == null));

            RunTest(@"
                assert(isnull(undefined) === true);
            ");
        }

        [TestMethod]
        public void DelegateWithObjectParameterCanBePassedAnUndefined()
        {
            _engine.SetValue("isnull", new Func<object, bool>(x => x == null));

            RunTest(@"
                assert(isnull(undefined) === true);
            ");
        }

        [TestMethod]
        public void DelegateWithNullableParameterCanBeExcluded()
        {
            _engine.SetValue("isnull", new Func<double?, bool>(x => x == null));

            RunTest(@"
                assert(isnull() === true);
            ");
        }

        [TestMethod]
        public void DelegateWithObjectParameterCanBeExcluded()
        {
            _engine.SetValue("isnull", new Func<object, bool>(x => x == null));

            RunTest(@"
                assert(isnull() === true);
            ");
        }

        [TestMethod]
        public void ExtraParametersAreIgnored()
        {
            _engine.SetValue("passNumber", new Func<int, int>(x => x));

            RunTest(@"
                assert(passNumber(123,'test',{},[],null) === 123);
            ");
        }

        private delegate string callParams(params object[] values);
        private delegate string callArgumentAndParams(string firstParam, params object[] values);

        [TestMethod]
        public void DelegatesWithParamsParameterCanBeInvoked() // TODO FS: CF handles Delegates different then Full .NET 3.5. DelegegateWrapper Method.Invoke throws probably because the parameters variable is set wrong for CF.
        {
            var a = new A();
            _engine.SetValue("callParams", new callParams(a.Call13));
            _engine.SetValue("callArgumentAndParams", new callArgumentAndParams(a.Call14));

            RunTest(@"
                assert(callParams('1','2','3') === '1,2,3');
                assert(callParams('1') === '1');
                assert(callParams() === '');

                assert(callArgumentAndParams('a','1','2','3') === 'a:1,2,3');
                assert(callArgumentAndParams('a','1') === 'a:1');
                assert(callArgumentAndParams('a') === 'a:');
                assert(callArgumentAndParams() === ':');
            ");
        }

        [TestMethod]
        public void CanGetObjectProperties()
        {
            var p = new Person
            {
                Name = "Mickey Mouse"
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Name === 'Mickey Mouse');
            ");
        }

        [TestMethod]
        public void CanInvokeObjectMethods()
        {
            var p = new Person
            {
                Name = "Mickey Mouse"
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.ToString() === 'Mickey Mouse');
            ");
        }

        [TestMethod]
        public void CanInvokeObjectMethodsWithPascalCase()
        {
            var p = new Person
            {
                Name = "Mickey Mouse"
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.toString() === 'Mickey Mouse');
            ");
        }

        [TestMethod]
        public void CanSetObjectProperties()
        {
            var p = new Person
            {
                Name = "Mickey Mouse"
            };

            _engine.SetValue("p", p);

            RunTest(@"
                p.Name = 'Donald Duck';
                assert(p.Name === 'Donald Duck');
            ");

            Assert.AreEqual("Donald Duck", p.Name);
        }

        [TestMethod]
        public void CanGetIndexUsingStringKey()
        {
            var dictionary = new Dictionary<string, Person>();
            dictionary.Add("person1", new Person { Name = "Mickey Mouse" });
            dictionary.Add("person2", new Person { Name = "Goofy" });

            _engine.SetValue("dictionary", dictionary);

            RunTest(@"
                assert(dictionary['person1'].Name === 'Mickey Mouse');
                assert(dictionary['person2'].Name === 'Goofy');
            ");
        }

        [TestMethod]
        public void CanSetIndexUsingStringKey()
        {
            var dictionary = new Dictionary<string, Person>();
            dictionary.Add("person1", new Person { Name = "Mickey Mouse" });
            dictionary.Add("person2", new Person { Name = "Goofy" });

            _engine.SetValue("dictionary", dictionary);

            RunTest(@"
                dictionary['person2'].Name = 'Donald Duck';
                assert(dictionary['person2'].Name === 'Donald Duck');
            ");

            Assert.AreEqual("Donald Duck", dictionary["person2"].Name);
        }

        [TestMethod]
        public void CanGetIndexUsingIntegerKey()
        {
            var dictionary = new Dictionary<int, string>();
            dictionary.Add(1, "Mickey Mouse");
            dictionary.Add(2, "Goofy");

            _engine.SetValue("dictionary", dictionary);

            RunTest(@"
                assert(dictionary[1] === 'Mickey Mouse');
                assert(dictionary[2] === 'Goofy');
            ");
        }

        [TestMethod]
        public void CanSetIndexUsingIntegerKey()
        {
            var dictionary = new Dictionary<int, string>();
            dictionary.Add(1, "Mickey Mouse");
            dictionary.Add(2, "Goofy");

            _engine.SetValue("dictionary", dictionary);

            RunTest(@"
                dictionary[2] = 'Donald Duck';
                assert(dictionary[2] === 'Donald Duck');
            ");

            Assert.AreEqual("Mickey Mouse", dictionary[1]);
            Assert.AreEqual("Donald Duck", dictionary[2]);
        }

        [TestMethod]
        public void CanUseGenericMethods()
        {
            var dictionary = new Dictionary<int, string>();
            dictionary.Add(1, "Mickey Mouse");


            _engine.SetValue("dictionary", dictionary);

            RunTest(@"
                dictionary.Add(2, 'Goofy');
                assert(dictionary[2] === 'Goofy');
            ");

            Assert.AreEqual("Mickey Mouse", dictionary[1]);
            Assert.AreEqual("Goofy", dictionary[2]);
        }

        [TestMethod]
        public void CanUseMultiGenericTypes()
        {

            RunTest(@"
                var type = System.Collections.Generic.Dictionary(System.Int32, System.String);
                var dictionary = new type();
                dictionary.Add(1, 'Mickey Mouse');
                dictionary.Add(2, 'Goofy');
                assert(dictionary[2] === 'Goofy');
            ");
        }

        [TestMethod]
        public void CanUseIndexOnCollection()
        {
            var collection = new System.Collections.ObjectModel.Collection<string>();
            collection.Add("Mickey Mouse");
            collection.Add("Goofy");

            _engine.SetValue("dictionary", collection);

            RunTest(@"
                dictionary[1] = 'Donald Duck';
                assert(dictionary[1] === 'Donald Duck');
            ");

            Assert.AreEqual("Mickey Mouse", collection[0]);
            Assert.AreEqual("Donald Duck", collection[1]);
        }

        [TestMethod]
        public void CanUseIndexOnList()
        {
            var arrayList = new System.Collections.ArrayList(2);
            arrayList.Add("Mickey Mouse");
            arrayList.Add("Goofy");

            _engine.SetValue("dictionary", arrayList);

            RunTest(@"
                dictionary[1] = 'Donald Duck';
                assert(dictionary[1] === 'Donald Duck');
            ");

            Assert.AreEqual("Mickey Mouse", arrayList[0]);
            Assert.AreEqual("Donald Duck", arrayList[1]);
        }

        [TestMethod]
        public void CanAccessAnonymousObject()
        {
            var p = new
            {
                Name = "Mickey Mouse",
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Name === 'Mickey Mouse');
            ");
        }

        [TestMethod]
        public void CanAccessAnonymousObjectProperties()
        {
            var p = new
            {
                Address = new
                {
                    City = "Mouseton"
                }
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Address.City === 'Mouseton');
            ");
        }

        [TestMethod]
        public void PocosCanReturnJsValueDirectly()
        {
            var o = new
            {
                x = new JsValue(1),
                y = new JsValue("string"),
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.x === 1);
                assert(o.y === 'string');
            ");
        }

        [TestMethod]
        public void PocosCanReturnObjectInstanceDirectly()
        {
            var x = new ObjectInstance(_engine) { Extensible = true };
            x.Put("foo", new JsValue("bar"), false);

            var o = new
            {
                x
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.x.foo === 'bar');
            ");
        }

        [TestMethod]
        public void DateTimeIsConvertedToDate()
        {
            var o = new
            {
                z = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.z.valueOf() === 0);
            ");
        }

        [TestMethod]
        public void DateTimeOffsetIsConvertedToDate() // CF has no DateTimeOffset so used DateTime instead. valueOf() only exists when DateTimeKind.Utc is set.
        {
            var o = new
            {
                z = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.z.valueOf() === 0);
            ");
        }

        [TestMethod]
        public void EcmaValuesAreAutomaticallyConvertedWhenSetInPoco()
        {
            var p = new Person
            {
                Name = "foo",
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Name === 'foo');
                assert(p.Age === 0);
                p.Name = 'bar';
                p.Age = 10;
            ");

            Assert.AreEqual("bar", p.Name);
            Assert.AreEqual(10, p.Age);
        }

        [TestMethod]
        public void EcmaValuesAreAutomaticallyConvertedToBestMatchWhenSetInPoco()
        {
            var p = new Person
            {
                Name = "foo",
            };

            _engine.SetValue("p", p);

            RunTest(@"
                p.Name = 10;
                p.Age = '20';
            ");

            Assert.AreEqual("10", p.Name);
            Assert.AreEqual(20, p.Age);
        }

        [TestMethod]
        public void ShouldCallInstanceMethodWithoutArgument()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call1() === 0);
            ");
        }

        [TestMethod]
        public void ShouldCallInstanceMethodOverloadArgument()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call1(1) === 1);
            ");
        }

        [TestMethod]
        public void ShouldCallInstanceMethodWithString()
        {
            var p = new Person();
            _engine.SetValue("a", new A());
            _engine.SetValue("p", p);

            RunTest(@"
                p.Name = a.Call2('foo');
                assert(p.Name === 'foo');
            ");

            Assert.AreEqual("foo", p.Name);
        }

        [TestMethod]
        public void CanUseTrim()
        {
            var p = new Person { Name = "Mickey Mouse " };
            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Name === 'Mickey Mouse ');
                p.Name = p.Name.trim();
                assert(p.Name === 'Mickey Mouse');
            ");

            Assert.AreEqual("Mickey Mouse", p.Name);
        }

        [TestMethod]
        public void CanUseMathFloor()
        {
            var p = new Person();
            _engine.SetValue("p", p);

            RunTest(@"
                p.Age = Math.floor(1.6);p
                assert(p.Age === 1);
            ");

            Assert.AreEqual(1, p.Age);
        }

        [TestMethod]
        public void CanUseDelegateAsFunction()
        {
            var even = new Func<int, bool>(x => x % 2 == 0);
            _engine.SetValue("even", even);

            RunTest(@"
                assert(even(2) === true);
            ");
        }

        [TestMethod]
        public void ShouldConvertArrayToArrayInstance()
        {
            var result = _engine
                .SetValue("values", new[] { 1, 2, 3, 4, 5, 6 })
                .Execute("values.filter(function(x){ return x % 2 == 0; })");

            var parts = result.GetCompletionValue().ToObject();

            Assert.IsTrue(parts.GetType().IsArray);
            Assert.AreEqual(3, ((object[])parts).Length);
            Assert.AreEqual(2d, ((object[])parts)[0]);
            Assert.AreEqual(4d, ((object[])parts)[1]);
            Assert.AreEqual(6d, ((object[])parts)[2]);
        }

        [TestMethod]
        public void ShouldConvertListsToArrayInstance()
        {
            var result = _engine
                .SetValue("values", new List<object> { 1, 2, 3, 4, 5, 6 })
                .Execute("new Array(values).filter(function(x){ return x % 2 == 0; })");

            var parts = result.GetCompletionValue().ToObject();

            Assert.IsTrue(parts.GetType().IsArray);
            Assert.AreEqual(3, ((object[])parts).Length);
            Assert.AreEqual(2d, ((object[])parts)[0]);
            Assert.AreEqual(4d, ((object[])parts)[1]);
            Assert.AreEqual(6d, ((object[])parts)[2]);
        }

        [TestMethod]
        public void ShouldConvertArrayInstanceToArray()
        {
            var result = _engine.Execute("'foo@bar.com'.split('@');");
            var parts = result.GetCompletionValue().ToObject();

            Assert.IsTrue(parts.GetType().IsArray);
            Assert.AreEqual(2, ((object[])parts).Length);
            Assert.AreEqual("foo", ((object[])parts)[0]);
            Assert.AreEqual("bar.com", ((object[])parts)[1]);
        }

        [TestMethod]
        public void ShouldConvertBooleanInstanceToBool()
        {
            var result = _engine.Execute("new Boolean(true)");
            var value = result.GetCompletionValue().ToObject();

            Assert.AreEqual(typeof(bool), value.GetType());
            Assert.AreEqual(true, value);
        }

        [TestMethod]
        public void ShouldConvertDateInstanceToDateTime()
        {
            var result = _engine.Execute("new Date(0)");
            var value = result.GetCompletionValue().ToObject();

            Assert.AreEqual(typeof(DateTime), value.GetType());
            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), value);
        }

        [TestMethod]
        public void ShouldConvertNumberInstanceToDouble()
        {
            var result = _engine.Execute("new Number(10)");
            var value = result.GetCompletionValue().ToObject();

            Assert.AreEqual(typeof(double), value.GetType());
            Assert.AreEqual(10d, value);
        }

        [TestMethod]
        public void ShouldConvertStringInstanceToString()
        {
            var result = _engine.Execute("new String('foo')");
            var value = result.GetCompletionValue().ToObject();

            Assert.AreEqual(typeof(string), value.GetType());
            Assert.AreEqual("foo", value);
        }

#if !__CF__
        [TestMethod]
        public void ShouldConvertObjectInstanceToExpando() 
        {
            _engine.Execute("var o = {a: 1, b: 'foo'}");
            var result = _engine.GetValue("o");

            dynamic value = result.ToObject();

            Assert.AreEqual(1, value.a);
            Assert.AreEqual("foo", value.b);

            var dic = (IDictionary<string, object>)result.ToObject();

            Assert.AreEqual(1d, dic["a"]);
            Assert.AreEqual("foo", dic["b"]);
        }
#endif

        [TestMethod]
        public void ShouldGetValueFromDictionary() // TODO FS : CHECK IF NO OTHER DICTIONARY TEST HAVE BEEN DEFINED....
        {
            var stringKey = new Dictionary<string, string>();
            stringKey.Add("test", "correct");
            stringKey.Add("foo", "bar");
            
            _engine.SetValue("dic", stringKey);
            RunTest(@"
                         assert(dic['test'] === 'correct');
                         assert(dic['foo'] === 'bar');
                    ");

            var integerKey = new Dictionary<int, string>();
            integerKey.Add(1, "correct");
            integerKey.Add(999, "bar");

            _engine.SetValue("dic", integerKey);
            RunTest(@"
                         assert(dic[1] === 'correct');
                         assert(dic[999] === 'bar');
                    ");
        }

        [TestMethod]
        public void ShouldSetValueInDictionary() // TODO FS : CHECK IF NO OTHER DICTIONARY TEST HAVE BEEN DEFINED....
        {
            var stringKey = new Dictionary<string, string>();
            stringKey.Add("test", "nothing");
            stringKey.Add("foo", "bar");

            _engine.SetValue("dic", stringKey);
            RunTest(@"
                         dic['test'] = 'correct';
                         assert(dic['test'] === 'correct');
                         assert(dic['foo'] === 'bar');
                    ");
        }

        [TestMethod]
        public void ShouldNotTryToConvertCompatibleTypes()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call3('foo') === 'foo');
                assert(a.Call3(1) === '1');
            ");
        }

        [TestMethod]
        public void ShouldNotTryToConvertDerivedTypes()
        {
            _engine.SetValue("a", new A());
            _engine.SetValue("p", new Person { Name = "Mickey" });

            RunTest(@"
                assert(a.Call4(p) === 'Mickey');
            ");
        }

        [TestMethod]
        public void ShouldExecuteFunctionCallBackAsDelegate()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call5(function(a,b){ return a+b }) === '1foo');
            ");
        }

        [TestMethod]
        public void ShouldExecuteFunctionCallBackAsFuncAndThisCanBeAssigned()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call6(function(a,b){ return this+a+b }) === 'bar1foo');
            ");
        }

        [TestMethod]
        public void ShouldExecuteFunctionCallBackAsPredicate()
        {
            _engine.SetValue("a", new A());

            // Func<>
            RunTest(@"
                assert(a.Call8(function(){ return 'foo'; }) === 'foo');
            ");
        }

        [TestMethod]
        public void ShouldExecuteFunctionWithParameterCallBackAsPredicate()
        {
            _engine.SetValue("a", new A());

            // Func<,>
            RunTest(@"
                assert(a.Call7('foo', function(a){ return a === 'foo'; }) === true);
            ");
        }
        
        [TestMethod]
        public void ShouldExecuteActionCallBackAsPredicate()
        {
            _engine.SetValue("a", new A());

            // Action
            RunTest(@"
                var value;
                a.Call9(function(){ value = 'foo'; });
                assert(value === 'foo');
            ");
        }

        [TestMethod]
        public void ShouldExecuteActionWithParameterCallBackAsPredicate()
        {
            _engine.SetValue("a", new A());

            // Action<>
            RunTest(@"
                var value;
                a.Call10('foo', function(b){ value = b; });
                assert(value === 'foo');
            ");
        }

        [TestMethod]
        public void ShouldExecuteActionWithMultipleParametersCallBackAsPredicate()
        {
            _engine.SetValue("a", new A());

            // Action<,>
            RunTest(@"
                var value;
                a.Call11('foo', 'bar', function(a,b){ value = a + b; });
                assert(value === 'foobar');
            ");
        }

        [TestMethod]
        public void ShouldExecuteFunc()
        {
            _engine.SetValue("a", new A());

            // Func<int, int>
            RunTest(@"
                var result = a.Call12(42, function(a){ return a + a; });
                assert(result === 84);
            ");
        }
        
        [TestMethod]
        public void ShouldExecuteActionCallBackOnEventChanges()
        {
            _engine.SetValue("button", new System.Windows.Forms.Button());

            _engine.Execute(@"
                var eventChanged;
                button.add_TextChanged(function(s,e) { eventChanged = true; });
                button.Text = 'foobar'
            ");

            var eventChanged = _engine.GetValue("eventChanged").AsBoolean();
            Assert.IsTrue(eventChanged);
        }

        [TestMethod]
        public void ShouldRemoveActionCallbackOnEventChanged()
        {
            _engine.SetValue("button", new System.Windows.Forms.Button());
            _engine.SetValue("clearEventInvocations", new Action<object, string>((object obj, string eventName) =>
            {
                FieldInfo field = null;
                Type type = obj.GetType();
                while (type != null)
                {
                    /* Find events defined as field */
                    field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                        break;

                    /* Find events defined as property { add; remove; } */
                    field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                        break;
                    type = type.BaseType;
                }
                if (field == null) return;
                field.SetValue(obj, null);
            }));

            RunTest(@"
                var eventCount = 0;
                var eventChanged;

                button.add_TextChanged(function(s,e) { eventChanged = true; eventCount += 1; });
                button.Text = 'foo'

                clearEventInvocations(button, 'TextChanged');
                button.Text = 'bar'
            ");

            var eventChanged = _engine.GetValue("eventChanged").AsBoolean();
            var eventCount = _engine.GetValue("eventCount").AsNumber();
            Assert.IsTrue(eventChanged);
            Assert.IsTrue(eventCount == 1);
        }

        [TestMethod]
        public void ShouldUseSystemIO()
        {
            RunTest(@"
                var filename = System.IO.Path.GetTempFileName();
                var sw = System.IO.File.CreateText(filename);
                sw.Write('Hello World');
                sw.Dispose();
                
                var content = System.IO.File.OpenText(filename).ReadToEnd();
                System.Console.WriteLine(content);
                
                assert(content === 'Hello World');
            ");

            // TODO Test if OpenText needs disposal
        }

        [TestMethod]
        public void ShouldImportNamespace()
        {
            RunTest(@"
                var Shapes = importNamespace('Shapes');
                var circle = new Shapes.Circle();
                assert(circle.Radius === 0);
                assert(circle.Perimeter() === 0);
            ");
        }

        [TestMethod]
        public void ShouldConstructWithParameters()
        {
            RunTest(@"
                var Shapes = importNamespace('Shapes');
                var circle = new Shapes.Circle(1);
                assert(circle.Radius === 1);
                assert(circle.Perimeter() === Math.PI);
            ");
        }

        [TestMethod]
        public void ShouldInvokeAFunctionByName()
        {
            RunTest(@"
                function add(x, y) { return x + y; }
            ");

            Assert.AreEqual(3, _engine.Invoke("add", 1, 2));
        }

        [TestMethod]
        public void ShouldNotInvokeNonFunctionValue() // TODO ????
        {
            RunTest(@"
                var x= 10;
            ");

            try
            {
                _engine.Invoke("x", 1, 2);

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
        public void CanGetField()
        {
            var o = new ClassWithField
            {
                Field = "Mickey Mouse"
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.Field === 'Mickey Mouse');
            ");
        }

        [TestMethod]
        public void CanSetField()
        {
            var o = new ClassWithField();

            _engine.SetValue("o", o);

            RunTest(@"
                o.Field = 'Mickey Mouse';
                assert(o.Field === 'Mickey Mouse');
            ");

            Assert.AreEqual("Mickey Mouse", o.Field);
        }

        [TestMethod]
        public void CanGetStaticField()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var statics = domain.ClassWithStaticFields;
                assert(statics.Get == 'Get');
            ");
        }

        [TestMethod]
        public void CanSetStaticField()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var statics = domain.ClassWithStaticFields;
                statics.Set = 'hello';
                assert(statics.Set == 'hello');
            ");

            Assert.AreEqual(ClassWithStaticFields.Set, "hello");
        }

        [TestMethod]
        public void CanGetStaticAccessor()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var statics = domain.ClassWithStaticFields;
                assert(statics.Getter == 'Getter');
            ");
        }

        [TestMethod]
        public void CanSetStaticAccessor()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var statics = domain.ClassWithStaticFields;
                statics.Setter = 'hello';
                assert(statics.Setter == 'hello');
            ");

            Assert.AreEqual(ClassWithStaticFields.Setter, "hello");
        }

        [TestMethod]
        public void CantSetStaticReadonly()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var statics = domain.ClassWithStaticFields;
                statics.Readonly = 'hello';
                assert(statics.Readonly == 'Readonly');
            ");

            Assert.AreEqual(ClassWithStaticFields.Readonly, "Readonly");
        }

        [TestMethod]
        public void CanSetCustomConverters()
        {

            var engine1 = new Engine();
            engine1.SetValue("p", new { Test = true });
            engine1.Execute("var result = p.Test;");
            Assert.IsTrue((bool)engine1.GetValue("result").ToObject());

            var engine2 = new Engine(o => o.AddObjectConverter(new NegateBoolConverter()));
            engine2.SetValue("p", new { Test = true });
            engine2.Execute("var result = p.Test;");
            Assert.IsFalse((bool)engine2.GetValue("result").ToObject());

        }

        [TestMethod]
        public void CanUserIncrementOperator()
        {
            var p = new Person
            {
                Age = 1,
            };

            _engine.SetValue("p", p);

            RunTest(@"
                assert(++p.Age === 2);
            ");

            Assert.AreEqual(2, p.Age);
        }

        [TestMethod]
        public void CanOverwriteValues()
        {
            _engine.SetValue("x", 3);
            _engine.SetValue("x", 4);

            RunTest(@"
                assert(x === 4);
            ");
        }

        [TestMethod]
        public void ShouldCreateGenericType()
        {
            RunTest(@"
                var ListOfString = System.Collections.Generic.List(System.String);
                var list = new ListOfString();
                list.Add('foo');
                list.Add(1);
                assert(2 === list.Count);
            ");
        }

        [TestMethod]
        public void EnumComparesByName()
        {
            var o = new
            {
                r = Colors.Red,
                b = Colors.Blue,
                g = Colors.Green,
                b2 = Colors.Red
            };

            _engine.SetValue("o", o);
            _engine.SetValue("assertFalse", new Action<bool>(Assert.IsFalse));

            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var colors = domain.Colors;
                assert(o.r === colors.Red);
                assert(o.g === colors.Green);
                assert(o.b === colors.Blue);
                assertFalse(o.b2 === colors.Blue);
            ");
        }

        [TestMethod]
        public void ShouldSetEnumProperty()
        {
            var s = new Circle
            {
                Color = Colors.Red,
            };

            _engine.SetValue("s", s);

            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain');
                var colors = domain.Colors;
                
                s.Color = colors.Blue;
                assert(s.Color === colors.Blue);
            ");

            _engine.SetValue("s", s);

            RunTest(@"
                s.Color = colors.Blue | colors.Green;
                assert(s.Color === colors.Blue | colors.Green);
            ");

            Assert.AreEqual(Colors.Blue | Colors.Green, s.Color);
        }

        [TestMethod]
        public void EnumIsConvertedToNumber()
        {
            var o = new
            {
                r = Colors.Red,
                b = Colors.Blue,
                g = Colors.Green
            };

            _engine.SetValue("o", o);

            RunTest(@"
                assert(o.r === 0);
                assert(o.g === 1);
                assert(o.b === 10);
            ");
        }


        [TestMethod]
        public void ShouldConvertToEnum()
        {
            var s = new Circle
            {
                Color = Colors.Red,
            };

            _engine.SetValue("s", s);

            RunTest(@"
                assert(s.Color === 0);
                s.Color = 10;
                assert(s.Color === 10);
            ");

            _engine.SetValue("s", s);

            RunTest(@"
                s.Color = 11;
                assert(s.Color === 11);
            ");

            Assert.AreEqual(Colors.Blue | Colors.Green, s.Color);
        }

        [TestMethod]
        public void ShouldUseExplicitPropertyGetter()
        {
            _engine.SetValue("c", new Company("ACME"));

            RunTest(@"
                assert(c.Name === 'ACME');
            ");
        }

        [TestMethod]
        public void ShouldUseExplicitIndexerPropertyGetter()
        {
            var company = new Company("ACME");
            ((ICompany)company)["Foo"] = "Bar";
            _engine.SetValue("c", company);

            RunTest(@"
                assert(c.Foo === 'Bar');
            ");
        }


        [TestMethod]
        public void ShouldUseExplicitPropertySetter()
        {
            _engine.SetValue("c", new Company("ACME"));

            RunTest(@"
                c.Name = 'Foo';
                assert(c.Name === 'Foo');
            ");
        }

        [TestMethod]
        public void ShouldUseExplicitIndexerPropertySetter()
        {
            var company = new Company("ACME");
            ((ICompany)company)["Foo"] = "Bar";
            _engine.SetValue("c", company);

            RunTest(@"
                c.Foo = 'Baz';
                assert(c.Foo === 'Baz');
            ");
        }


        [TestMethod]
        public void ShouldUseExplicitMethod()
        {
            _engine.SetValue("c", new Company("ACME"));

            RunTest(@"
                assert(0 === c.CompareTo(c));
            ");
        }

        [TestMethod]
        public void ShouldCallInstanceMethodWithParams()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call13('1','2','3') === '1,2,3');
                assert(a.Call13('1') === '1');
                assert(a.Call13(1) === '1');
                assert(a.Call13() === '');

                assert(a.Call14('a','1','2','3') === 'a:1,2,3');
                assert(a.Call14('a','1') === 'a:1');
                assert(a.Call14('a') === 'a:');

                function call13wrapper(){ return a.Call13.apply(a, Array.prototype.slice.call(arguments)); }
                assert(call13wrapper('1','2','3') === '1,2,3');

                assert(a.Call13('1','2','3') === a.Call13(['1','2','3']));
            ");
        }

        [TestMethod]
        public void ShouldCallInstanceMethodWithJsValueParams()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                assert(a.Call16('1','2','3') === '1,2,3');
                assert(a.Call16('1') === '1');
                assert(a.Call16(1) === '1');
                assert(a.Call16() === '');
                assert(a.Call16('1','2','3') === a.Call16(['1','2','3']));
            ");
        }

        [TestMethod]
        public void NullValueAsArgumentShouldWork()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                var x = a.Call2(null);
                assert(x === null);
            ");
        }

        [TestMethod]
        public void ShouldSetPropertyToNull()
        {
            var p = new Person { Name = "Mickey" };
            _engine.SetValue("p", p);

            RunTest(@"
                assert(p.Name != null);
                p.Name = null;
                assert(p.Name == null);
            ");

            Assert.IsTrue(p.Name == null);
        }

        [TestMethod]
        public void ShouldCallMethodWithNull()
        {
            _engine.SetValue("a", new A());

            RunTest(@"
                a.Call15(null);
                var result = a.Call2(null);
                assert(result == null);
            ");
        }

        [TestMethod]
        public void ShouldReturnUndefinedProperty()
        {
            _engine.SetValue("uo", new { foo = "bar" });
            _engine.SetValue("ud", new Dictionary<string, object>() { { "foo", "bar" } });
            _engine.SetValue("ul", new List<string>() { "foo", "bar" });

            RunTest(@"
                assert(!uo.undefinedProperty);
                assert(!ul[5]);
                assert(!ud.undefinedProperty);
            ");
        }

        [TestMethod]
        public void ShouldAutomaticallyConvertArraysToFindBestInteropResulution() // TODO FS: Somehow CF uses the ArrayConverterItem.ToType... See how this can be fixed for CF. Full .NET 3.5 doesn't have this problem....
        {
            _engine.SetValue("a", new ArrayConverterTestClass());
            _engine.SetValue("item1", new ArrayConverterItem(1));
            _engine.SetValue("item2", new ArrayConverterItem(2));

            RunTest(@"
                assert(a.MethodAcceptsArrayOfInt([false, '1', 2]) === a.MethodAcceptsArrayOfInt([0, 1, 2]));
                assert(a.MethodAcceptsArrayOfStrings(['1', 2]) === a.MethodAcceptsArrayOfStrings([1, 2]));
                assert(a.MethodAcceptsArrayOfBool(['1', 0]) === a.MethodAcceptsArrayOfBool([true, false]));

                assert(a.MethodAcceptsArrayOfStrings([item1, item2]) === a.MethodAcceptsArrayOfStrings(['1', '2']));
                assert(a.MethodAcceptsArrayOfInt([item1, item2]) === a.MethodAcceptsArrayOfInt([1, 2]));
            ");
        }

        [TestMethod]
        public void ShouldImportNamespaceNestedType()
        {
            RunTest(@"
                var shapes = importNamespace('Shapes.Circle');
                var kinds = shapes.Kind;
                assert(kinds.Unit === 0);
                assert(kinds.Ellipse === 1);
                assert(kinds.Round === 5);
            ");
        }

        [TestMethod]
        public void ShouldImportNamespaceNestedNestedType()
        {
            RunTest(@"
                var meta = importNamespace('Shapes.Circle.Meta');
                var usages = meta.Usage;
                assert(usages.Public === 0);
                assert(usages.Private === 1);
                assert(usages.Internal === 11);
            ");
        }

        [TestMethod]
        public void ShouldGetNestedNestedProp()
        {
            RunTest(@"
                var meta = importNamespace('Shapes.Circle');
                var m = new meta.Meta();
                assert(m.Description === 'descp');
            ");
        }

        [TestMethod]
        public void ShouldSetNestedNestedProp()
        {
            RunTest(@"
                var meta = importNamespace('Shapes.Circle');
                var m = new meta.Meta();
                m.Description = 'hello';
                assert(m.Description === 'hello');
            ");
        }

        [TestMethod]
        public void CanGetStaticNestedField()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain.Nested');
                var statics = domain.ClassWithStaticFields;
                assert(statics.Get == 'Get');
            ");
        }

        [TestMethod]
        public void CanSetStaticNestedField()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain.Nested');
                var statics = domain.ClassWithStaticFields;
                statics.Set = 'hello';
                assert(statics.Set == 'hello');
            ");

            Assert.AreEqual(Nested.ClassWithStaticFields.Set, "hello");
        }

        [TestMethod]
        public void CanGetStaticNestedAccessor()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain.Nested');
                var statics = domain.ClassWithStaticFields;
                assert(statics.Getter == 'Getter');
            ");
        }

        [TestMethod]
        public void CanSetStaticNestedAccessor()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain.Nested');
                var statics = domain.ClassWithStaticFields;
                statics.Setter = 'hello';
                assert(statics.Setter == 'hello');
            ");

            Assert.AreEqual(Nested.ClassWithStaticFields.Setter, "hello");
        }

        [TestMethod]
        public void CantSetStaticNestedReadonly()
        {
            RunTest(@"
                var domain = importNamespace('Jint.Tests.Runtime.Domain.Nested');
                var statics = domain.ClassWithStaticFields;
                statics.Readonly = 'hello';
                assert(statics.Readonly == 'Readonly');
            ");

            Assert.AreEqual(Nested.ClassWithStaticFields.Readonly, "Readonly");
        }
    }
}
