using System;
using Jint.Native;
using System.Linq;

namespace Jint.Tests.Runtime.Domain
{
    public class A
    {
        public int Call1()
        {
            return 0;
        }

        public int Call1(int x)
        {
            return x;
        }

        public string Call2(string x)
        {
            return x;
        }

        public string Call3(object x)
        {
            return x.ToString();
        }

        public string Call4(IPerson x)
        {
            return x.ToString();
        }

        public string Call5(Delegate callback) // Orignal way by Invoke fails with InvalidProgramException: This is a bug in the .NET compiler (http://stackoverflow.com/questions/5243065/fun-with-delegate-casting-invalidprogramexception)
        {
            var thisArg = JsValue.Undefined;
            var arguments = new JsValue[] { 1, "foo" };
            
            //return callback.Method.Invoke(thisArg, new object[] { arguments }).ToString();
            return ((Func<JsValue, JsValue[], JsValue>)callback)(thisArg, arguments).ToString();
        }

        public string Call6(Func<JsValue, JsValue[], JsValue> callback)
        {
            var thisArg = new JsValue("bar");
            var arguments = new JsValue[] { 1, "foo" };

            return callback(thisArg, arguments).ToString();
        }

        public bool Call7(string str, Func<string, bool> predicate)
        {
            return predicate(str);
        }

        public string Call8(Func<string> predicate)
        {
            return predicate();
        }

        public void Call9(Action predicate)
        {
            predicate();
        }
        
        public void Call10(string str, Action<string> predicate)
        {
            predicate(str);
        }

        public void Call11(string str, string str2, Action<string, string> predicate)
        {
            predicate(str, str2);
        }

        public int Call12(int value, Func<int, int> map)
        {
            return map(value);
        }

        public string Call13(params object[] values)
        {
            var stringValues = Enumerable.Select(values, item => item.ToString()).ToArray();
            return String.Join(",", stringValues);
        }

        public string Call14(string firstParam, params object[] values)
        {
            var stringValues = Enumerable.Select(values, item => item.ToString()).ToArray();
            return String.Format("{0}:{1}", firstParam, String.Join(",", stringValues));
        }

        public void Call15(string x)
        {

        }
        public string Call16(params JsValue[] values)
        {
            var stringValues = Enumerable.Select(values, item => item.ToString()).ToArray();
            return String.Join(",", stringValues);
        }
    }
}
