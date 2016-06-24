using System;
using Jint.Native.Object;
using Jint.Runtime.Interop;

namespace Jint.Native.Date
{
	public class TimeSpanInstance : ObjectInstance
	{
		private readonly TimeSpan _timeSpan;

		public TimeSpanInstance(Engine engine) : base(engine)
		{
			_timeSpan = new TimeSpan(DateTime.Now.Ticks);
		}

		public TimeSpanInstance(Engine engine, TimeSpan t) : base(engine)
		{
			_timeSpan = t;

			FastAddProperty("TotalDays", _timeSpan.TotalDays, true, false, true);
			FastAddProperty("TotalHours", _timeSpan.TotalHours, true, false, true);
			FastAddProperty("TotalMinutes", _timeSpan.TotalMinutes, true, false, true);
			FastAddProperty("TotalSeconds", _timeSpan.TotalSeconds, true, false, true);
			FastAddProperty("TotalMilliseconds", _timeSpan.TotalMilliseconds, true, false, true);
			FastAddProperty("Ticks", _timeSpan.Ticks, true, false, true);
			FastAddProperty("Days", _timeSpan.Days, true, false, true);
			FastAddProperty("Hours", _timeSpan.Hours, true, false, true);
			FastAddProperty("Minutes", _timeSpan.Minutes, true, false, true);
			FastAddProperty("Seconds", _timeSpan.Seconds, true, false, true);
			FastAddProperty("Milliseconds", _timeSpan.Milliseconds, true, false, true);
			FastAddProperty("toString", new ClrFunctionInstance(engine, ToString, 0), true, false, true);
			FastAddProperty("ToString", new ClrFunctionInstance(engine, ToString, 0), true, false, true);
		}

		public JsValue ToString(JsValue thisObj, JsValue[] arguments)
		{
			return _timeSpan.ToString();
		}
	}
}