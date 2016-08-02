﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Function;

namespace Jint.Runtime.Interop
{
    public sealed class MethodInfoFunctionInstance : FunctionInstance
    {
        private readonly MethodInfo[] _methods;
	    private Engine _engine;

        public MethodInfoFunctionInstance(Engine engine, MethodInfo[] methods)
            : base(engine, null, null, false)
        {
            _methods = methods;
	        _engine = engine;
            Prototype = engine.Function.PrototypeObject;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return Invoke(_methods, thisObject, arguments);
        }

        public JsValue Invoke(MethodInfo[] methodInfos, JsValue thisObject, JsValue[] jsArguments)
        {
            var arguments = ProcessParamsArrays(jsArguments, methodInfos);
            var methods = TypeConverter.FindBestMatch(Engine, methodInfos, arguments).ToList();
            var converter = Engine.ClrTypeConverter;

            foreach (var method in methods)
            {
                if (method.IsGenericMethodDefinition)
                {
                    Type[] typeArguments = new Type[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        typeArguments[i] = arguments[i].ToObject() as Type;
                    }
                    var genericMethod = (method as MethodInfo).MakeGenericMethod(typeArguments);
                    return JsValue.FromObject(Engine, genericMethod);
                }
                var parameters = new object[arguments.Length];
                var argumentsMatch = true;

                for (var i = 0; i < arguments.Length; i++)
                {
                    var parameterType = method.GetParameters()[i].ParameterType;

                    if (parameterType == typeof(JsValue))
                    {
                        parameters[i] = arguments[i];
                    }
                    else if (parameterType == typeof(JsValue[]) && arguments[i].IsArray())
                    {
                        // Handle specific case of F(params JsValue[])

                        var arrayInstance = arguments[i].AsArray();
                        var len = TypeConverter.ToInt32(arrayInstance.Get("length"));
                        var result = new JsValue[len];
                        for (var k = 0; k < len; k++)
                        {
                            var pk = k.ToString();
                            result[k] = arrayInstance.HasProperty(pk)
                                ? arrayInstance.Get(pk)
                                : JsValue.Undefined;
                        }

                        parameters[i] = result;
                    }
                    else
                    {
						if (!converter.TryConvert(arguments[i].ToObject(), parameterType, CultureInfo.InvariantCulture, out parameters[i]))
                        {
                            argumentsMatch = false;
                            break;
                        }
						_engine.LogDebug("now convert that return value to a lambda expression");
                        var lambdaExpression = parameters[i] as LambdaExpression;
						_engine.LogDebug("sweet! it was converted");
						if (lambdaExpression != null)
						{
							_engine.LogDebug("now let's see if we can call compile");
							parameters[i] = lambdaExpression.Compile();
							_engine.LogDebug("we totally rock because we called compile on the lambda");

#if __CF__
                            var isEventHandler = !(parameterType.Name.StartsWith("Func") || parameterType.Name.StartsWith("Action"));
                            if (isEventHandler)
                            {
                                var eventHandler = (Delegate)parameters[i];
                                parameters[i] = Delegate.CreateDelegate(parameterType, eventHandler.Target, eventHandler.Method);
                            }
#endif
						}
                    }
                }

                if (!argumentsMatch)
                {
                    continue;
                }

				// todo: cache method info

				_engine.LogDebug("let's give the invoke a try shall we?");
				return JsValue.FromObject(Engine, method.Invoke(method.IsStatic ? null : thisObject.ToObject(), parameters.ToArray()));
            }

			throw new JavaScriptException(Engine.TypeError, "No public methods with the specified arguments were found.");
        }

        /// <summary>
        /// Reduces a flat list of parameters to a params array
        /// </summary>
        private JsValue[] ProcessParamsArrays(JsValue[] jsArguments, IEnumerable<MethodInfo> methodInfos)
        {
            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
                if (!parameters.Any(p => Attribute.IsDefined(p, typeof(ParamArrayAttribute))))
                    continue;

                var nonParamsArgumentsCount = parameters.Length - 1;
                if (jsArguments.Length < nonParamsArgumentsCount)
                    continue;

                var newArgumentsCollection = jsArguments.Take(nonParamsArgumentsCount).ToList();
                var argsToTransform = jsArguments.Skip(nonParamsArgumentsCount).ToList();

                if (argsToTransform.Count == 1 && argsToTransform.FirstOrDefault().IsArray())
                    continue;

                var jsArray = Engine.Array.Construct(Arguments.Empty);
                Engine.Array.PrototypeObject.Push(jsArray, argsToTransform.ToArray());

                newArgumentsCollection.Add(new JsValue(jsArray));
                return newArgumentsCollection.ToArray();
            }

            return jsArguments;
        }

    }
}
