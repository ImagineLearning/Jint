using System;
using Jint.Native;
using Jint.Runtime.Interop;

namespace Jint.Runtime.Descriptors.Specialized
{
    public sealed class ClrAccessDescriptor : PropertyDescriptor
    {
        public ClrAccessDescriptor(Engine engine, Func<JsValue, JsValue> get)
            : this(engine, get, null)
        {
        }

#if __CF__
        public ClrAccessDescriptor(Engine engine, Func<JsValue, JsValue> get, Action<JsValue, JsValue> set)
            : base(
                new GetterFunctionInstance(engine, get),
                set == null ? Native.Undefined.Instance : new SetterFunctionInstance(engine, set)
                )
        {
        }
#else
        public ClrAccessDescriptor(Engine engine, Func<JsValue, JsValue> get, Action<JsValue, JsValue> set)
            : base(
                get: new GetterFunctionInstance(engine, get),
                set: set == null ? Native.Undefined.Instance : new SetterFunctionInstance(engine, set)
                )
        {
        }
#endif
    }
}
