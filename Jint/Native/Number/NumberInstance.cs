using System;
using Jint.Native.Object;
using Jint.Runtime;

namespace Jint.Native.Number
{
    public class NumberInstance : ObjectInstance, IPrimitiveInstance
    {
#if __CF__
        private static readonly long NegativeZeroBits = BitConverter.ToInt64(BitConverter.GetBytes(-0.0), 0);
#else
        private static readonly long NegativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);
#endif

        public NumberInstance(Engine engine)
            : base(engine)
        {
        }

        public override string Class
        {
            get
            {
                return "Number";
            }
        }

        Types IPrimitiveInstance.Type
        {
            get { return Types.Number; }
        }

        JsValue IPrimitiveInstance.PrimitiveValue
        {
            get { return PrimitiveValue; }
        }

        public JsValue PrimitiveValue { get; set; }

        public static bool IsNegativeZero(double x)
        {
#if __CF__
            return x == 0 && BitConverter.ToInt64(BitConverter.GetBytes(x), 0) == NegativeZeroBits;
#else
            return x == 0 && BitConverter.DoubleToInt64Bits(x) == NegativeZeroBits;
#endif
        }

        public static bool IsPositiveZero(double x)
        {
#if __CF__
            return x == 0 && BitConverter.ToInt64(BitConverter.GetBytes(x), 0) != NegativeZeroBits;
#else
            return x == 0 && BitConverter.DoubleToInt64Bits(x) != NegativeZeroBits;
#endif
        }

    }
}
