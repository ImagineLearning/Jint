using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Jint.Native;
using Jint.Runtime.Interop;

namespace Jint
{
    public class Options
    {
        private bool _discardGlobal;
        private bool _strict;
        private bool _allowDebuggerStatement;
        private bool _debugMode;
        private bool _allowClr;
        private readonly List<IObjectConverter> _objectConverters = new List<IObjectConverter>();
        private int _maxStatements;
        private int _maxRecursionDepth = -1; 
        private TimeSpan _timeoutInterval;
        private CultureInfo _culture = CultureInfo.CurrentCulture;
#if !__CF__
	    private TimeZoneInfo _localTimeZone = TimeZoneInfo.CreateCustomTimeZone("Mountain Standard Time",
		    TimeSpan.FromHours(-7), "Mountain Standard Time", "Mountain Standard Time");
#endif
		private List<Assembly> _lookupAssemblies = new List<Assembly>(); 

        /// <summary>
        /// When called, doesn't initialize the global scope.
        /// Can be useful in lightweight scripts for performance reason.
        /// </summary>
#if __CF__
        public Options DiscardGlobal() 
        { 
            return DiscardGlobal(true);
        }
        public Options DiscardGlobal(bool discard)
#else
        public Options DiscardGlobal(bool discard = true)
#endif
        {
            _discardGlobal = discard;
            return this;
        }

        /// <summary>
        /// Run the script in strict mode.
        /// </summary>
#if __CF__
        public Options Strict()
        {
            return Strict(true);
        }
        public Options Strict(bool strict)
#else
        public Options Strict(bool strict = true)
#endif
        {
            _strict = strict;
            return this;
        }

        /// <summary>
        /// Allow the <code>debugger</code> statement to be called in a script.
        /// </summary>
        /// <remarks>
        /// Because the <code>debugger</code> statement can start the 
        /// Visual Studio debugger, is it disabled by default
        /// </remarks>
#if __CF__
        public Options AllowDebuggerStatement()
        {
            return AllowDebuggerStatement(true);
        }
        public Options AllowDebuggerStatement(bool allowDebuggerStatement)
#else
        public Options AllowDebuggerStatement(bool allowDebuggerStatement = true)
#endif
        {
            _allowDebuggerStatement = allowDebuggerStatement;
            return this;
        }

        /// <summary>
        /// Allow to run the script in debug mode.
        /// </summary>
#if __CF__
        public Options DebugMode()
        {
            return DebugMode(true);
        }
        public Options DebugMode(bool debugMode)
#else
        public Options DebugMode(bool debugMode = true)
#endif
        {
            _debugMode = debugMode;
            return this;
        }

        /// <summary>
         /// Adds a <see cref="IObjectConverter"/> instance to convert CLR types to <see cref="JsValue"/>
        /// </summary>
        public Options AddObjectConverter(IObjectConverter objectConverter)
        {
            _objectConverters.Add(objectConverter);
            return this;
        }

        /// <summary>
        /// Allows scripts to call CLR types directly like <example>System.IO.File</example>
        /// </summary>
        public Options AllowClr(params Assembly[] assemblies)
        {
            _allowClr = true;
            _lookupAssemblies.AddRange(assemblies);
            _lookupAssemblies = _lookupAssemblies.Distinct().ToList();
            return this;
        }

#if __CF__
        public Options MaxStatements()
        {
            return MaxStatements(0);
        }
        public Options MaxStatements(int maxStatements)
#else
        public Options MaxStatements(int maxStatements = 0)
#endif
        {
            _maxStatements = maxStatements;
            return this;
        }
        
        public Options TimeoutInterval(TimeSpan timeoutInterval)
        {
            _timeoutInterval = timeoutInterval;
            return this;
        }

        /// <summary>
        /// Sets maximum allowed depth of recursion.
        /// </summary>
        /// <param name="maxRecursionDepth">
        /// The allowed depth.
        /// a) In case max depth is zero no recursion is allowed.
        /// b) In case max depth is equal to n it means that in one scope function can be called no more than n times.
        /// </param>
        /// <returns>Options instance for fluent syntax</returns>
#if __CF__
        public Options LimitRecursion()
        {
            return LimitRecursion(0);
        }
        public Options LimitRecursion(int maxRecursionDepth)
#else
        public Options LimitRecursion(int maxRecursionDepth = 0)
#endif
        {
            _maxRecursionDepth = maxRecursionDepth;
            return this;
        }

        public Options Culture(CultureInfo cultureInfo)
        {
            _culture = cultureInfo;
            return this;
        }
        
#if !__CF__
        public Options LocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            _localTimeZone = timeZoneInfo;
            return this;
        }
#endif

        internal bool GetDiscardGlobal()
        {
            return _discardGlobal;
        }

        internal bool IsStrict()
        {
            return _strict;
        }

        internal bool IsDebuggerStatementAllowed()
        {
            return _allowDebuggerStatement;
        }

        internal bool IsDebugMode()
        {
            return _debugMode;
        }

        internal bool IsClrAllowed()
        {
            return _allowClr;
        }
        
        internal IList<Assembly> GetLookupAssemblies()
        {
            return _lookupAssemblies;
        }

        internal IEnumerable<IObjectConverter> GetObjectConverters()
        {
            return _objectConverters;
        }

        internal int GetMaxStatements()
        {
            return _maxStatements;
        }

        internal int GetMaxRecursionDepth()
        {
            return _maxRecursionDepth;
        }

        internal TimeSpan GetTimeoutInterval()
        {
            return _timeoutInterval;
        }

        internal CultureInfo GetCulture()
        {
            return _culture;
        }

#if __CF__
        internal TimeZone GetLocalTimeZone()
        {
            return TimeZone.CurrentTimeZone;
        }
#else
        internal TimeZoneInfo GetLocalTimeZone()
        {
            return _localTimeZone;
        }
#endif
    }
}
