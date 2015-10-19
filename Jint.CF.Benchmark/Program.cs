using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Jint.Example
{
    static class Program
    {
        private const string Script = @"
            var o = {};
            o.Foo = 'bar';
            o.Baz = 42.0001;
            o.Blah = o.Foo + o.Baz;

            if(o.Blah != 'bar42.0001') throw TypeError;

            function fib(n){
                if(n<2) { 
                    return n; 
                }
    
                return fib(n-1) + fib(n-2);  
            }

            if(fib(3) != 2) throw TypeError;
        ";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            const int iterations = 1000;

            var watch = new System.Diagnostics.Stopwatch();

            Engine jint;
            jint = new Engine();
            jint.Execute(Script);

            watch.Reset();
            watch.Start();
            for (var i = 0; i < iterations; i++)
            {
                jint = new Jint.Engine();
                jint.Execute(Script);
            }
            var firstBenchMarkResult = string.Format("Jint: {0} iterations in {1} ms", iterations, watch.ElapsedMilliseconds);


            watch.Reset();
            watch.Start();
            for (var i = 0; i < iterations; i++)
            {
                jint.Execute(Script);
            }
            var secondBenchMarkResult = string.Format("Jint (reuse): {0} iterations in {1} ms", iterations, watch.ElapsedMilliseconds);
            MessageBox.Show(firstBenchMarkResult + Environment.NewLine + secondBenchMarkResult);
        }
    }
}