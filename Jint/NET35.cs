using System.Reflection;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static bool HasFlag(this FieldAttributes currentFieldAttributes, FieldAttributes flag)
    {
        return (currentFieldAttributes & flag) == flag;
    }

#if __CF__
    public static TimeSpan BaseUtcOffset(this TimeZone value)
    {
        return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
    }

    public static string ToUpperInvariant(this string value)
    {
        return value.ToUpper(CultureInfo.InvariantCulture);
    }

    public static string ToLowerInvariant(this string value)
    {
        return value.ToLower(CultureInfo.InvariantCulture);
    }

    public static string[] Split(this string value, string[] separator, StringSplitOptions options)
    {
        return value.Split(separator.Select(s=> Convert.ToChar(s)).ToArray(), options);
    }

    public static string[] Split(this string value, char[] separator, StringSplitOptions options)
    {
        if (options == StringSplitOptions.RemoveEmptyEntries)
            return value.Split(separator).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        else
            return value.Split(separator);
    }

    public static bool TryParse(this uint value, String key, out uint result)
    {
        try
        {
            result = uint.Parse(key);
            return true;
        }
        catch
        {
            result = uint.MinValue;
            return false;
        }
    }

    public static bool TryParse(this DateTime dt, String s, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
    {
        try
        {
            result = DateTime.Parse(s, provider, styles);
            return true;
        }
        catch
        {
            result = DateTime.MinValue;
            return false;
        }
    }

    public static bool TryParseExact(this DateTime dt, String s, string[] formats, IFormatProvider provider, DateTimeStyles styles, out DateTime result)
    {
        foreach (var format in formats)
        {
            try
            {
                result = DateTime.ParseExact(s, format, provider, styles);
                return true;
            }
            catch
            {
            }
        }

        result = DateTime.MinValue;
        return false;
    }

    public static Enum[] GetEnumValues(this Type et)
    {
        ////get the enumeration type
        //Type et = enumeration.GetType();

        //get the public static fields (members of the enum)
        System.Reflection.FieldInfo[] fi = et.GetFields(BindingFlags.Static | BindingFlags.Public);

        //create a new enum array
        Enum[] values = new Enum[fi.Length];

        //populate with the values
        for (int iEnum = 0; iEnum < fi.Length; iEnum++)
        {
            values[iEnum] = (Enum)fi[iEnum].GetValue(et);
        }

        //return the array
        return values;
    }

    public static string[] GetEnumNames(this Type et)
    {
        ////get the enumeration type
        //Type et = enumeration.GetType();

        //get the public static fields (members of the enum)
        System.Reflection.FieldInfo[] fi = et.GetFields(BindingFlags.Static | BindingFlags.Public);

        //create a new enum array
        string[] values = new string[fi.Length];

        //populate with the values
        for (int iEnum = 0; iEnum < fi.Length; iEnum++)
        {
            values[iEnum] = fi[iEnum].Name;
        }

        //return the array
        return values;
    }
#endif
}



#if __CF__
public enum StringSplitOptions { None, RemoveEmptyEntries }

namespace Jint
{
    [ConditionalAttribute("CONTRACTS_FULL")]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class DebuggerTypeProxy : Attribute
    {
        public DebuggerTypeProxy(Type type) { }
    }

    [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class ThreadStaticAttribute : Attribute
    {
    }
}
#endif



namespace Jint
{
    [ConditionalAttribute("CONTRACTS_FULL")]
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.Delegate, AllowMultiple = false, Inherited = true)]
    public sealed class PureAttribute : Attribute
    {
    }
}