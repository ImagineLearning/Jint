using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Date;
using Jint.Native.Object;
using Jint.Native.RegExp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jint.Tests.Runtime
{
    public class JsValueConversionTests
    {
        [TestMethod]
        public void ShouldBeAnArray()
        {
            var value = new JsValue(new ArrayInstance(null));
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(true, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(true, value.IsObject());
            Assert.AreEqual(false, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());

            Assert.AreEqual(true, value.AsArray() != null);
        }

        [TestMethod]
        public void ShouldBeABoolean()
        {
            var value = new JsValue(true);
            Assert.AreEqual(true, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(false, value.IsObject());
            Assert.AreEqual(true, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());

            Assert.AreEqual(true, value.AsBoolean());
        }

        [TestMethod]
        public void ShouldBeADate()
        {
            var value = new JsValue(new DateInstance(null));
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(true, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(true, value.IsObject());
            Assert.AreEqual(false, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());

            Assert.AreEqual(true, value.AsDate() != null);
        }

        [TestMethod]
        public void ShouldBeNull()
        {
            var value = Null.Instance;
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(true, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(false, value.IsObject());
            Assert.AreEqual(true, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());
        }

        [TestMethod]
        public void ShouldBeANumber()
        {
            var value = new JsValue(2);
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(true, value.IsNumber());
            Assert.AreEqual(2, value.AsNumber());
            Assert.AreEqual(false, value.IsObject());
            Assert.AreEqual(true, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());
        }

        [TestMethod]
        public void ShouldBeAnObject()
        {
            var value = new JsValue(new ObjectInstance(null));
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(true, value.IsObject());
            Assert.AreEqual(true, value.AsObject() != null);
            Assert.AreEqual(false, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());
        }

        [TestMethod]
        public void ShouldBeARegExp()
        {
            var value = new JsValue(new RegExpInstance(null));
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(true, value.IsObject());
            Assert.AreEqual(false, value.IsPrimitive());
            Assert.AreEqual(true, value.IsRegExp());
            Assert.AreEqual(true, value.AsRegExp() != null);
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(false, value.IsUndefined());
        }

        [TestMethod]
        public void ShouldBeAString()
        {
            var value = new JsValue("a");
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(false, value.IsObject());
            Assert.AreEqual(true, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(true, value.IsString());
            Assert.AreEqual("a", value.AsString());
            Assert.AreEqual(false, value.IsUndefined());
        }

        [TestMethod]
        public void ShouldBeUndefined()
        {
            var value = Undefined.Instance;
            Assert.AreEqual(false, value.IsBoolean());
            Assert.AreEqual(false, value.IsArray());
            Assert.AreEqual(false, value.IsDate());
            Assert.AreEqual(false, value.IsNull());
            Assert.AreEqual(false, value.IsNumber());
            Assert.AreEqual(false, value.IsObject());
            Assert.AreEqual(true, value.IsPrimitive());
            Assert.AreEqual(false, value.IsRegExp());
            Assert.AreEqual(false, value.IsString());
            Assert.AreEqual(true, value.IsUndefined());
        }
    }
}
