//using System;
//using System.Net;
//using System.Threading;
//using Jint.Runtime;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.IO;
//using System.Security.Cryptography.X509Certificates;

//namespace Jint.Tests.CommonScripts
//{
//    [TestClass]
//    public class SunSpiderTests
//    {
//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        //
//        // You can use the following additional attributes as you write your tests:
//        //
//        // Use ClassInitialize to run code before running the first test in the class
//        // [ClassInitialize()]
//        // public static void MyClassInitialize(TestContext testContext) { }
//        //
//        // Use ClassCleanup to run code after all tests in a class have run
//        // [ClassCleanup()]
//        // public static void MyClassCleanup() { }
//        //
//        // Use TestInitialize to run code before running each test 
//        // [TestInitialize()]
//        // public void MyTestInitialize() { }
//        //
//        // Use TestCleanup to run code after each test has run
//        // [TestCleanup()]
//        // public void MyTestCleanup() { }
//        //
//        #endregion


//        private Engine RunTest(string source)
//        {
//            var engine = new Engine()
//                .SetValue("log", new Action<object>(x => TestContext.WriteLine(x.ToString())))
//                .SetValue("assert", new Action<bool>(Assert.IsTrue))
//                ;

//            try
//            {
//                engine.Execute(source);
//            }
//            catch (JavaScriptException je)
//            {
//                throw new Exception(je.ToString());
//            }

//            return engine;
//        }

//        /// <summary>
//        /// Internal object used to allow setting WebRequest.CertificatePolicy to 
//        /// not fail on Cert errors
//        /// </summary>
//        internal class AcceptAllCertificatePolicy : ICertificatePolicy
//        {
//            public AcceptAllCertificatePolicy()
//            {

//            }
//            public bool CheckValidationResult(ServicePoint sPoint, X509Certificate cert, WebRequest wRequest, int certProb)
//            {
//                // *** Always accept
//                return true;
//            }
//        }

//        private string GetJavaScript(string uri)
//        {
//            try
//            {
//                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
//                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
//                using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
//                {
//                    using (Stream stream = httpResponse.GetResponseStream())
//                    {
//                        /*
//                        now you can use the stream so perform and read write operation
//                        eg:
//                        StreamReader reader = new StreamReader(stream);
//                        string responseData = reader.ReadLine();
//                        while(responseData !=null)
//                        {
//                        responseData +=reader.ReadLine();
//                        }
//                        */
//                        string responseData = string.Empty;
//                        using (StreamReader reader = new StreamReader(stream))
//                        {
//                            responseData = reader.ReadLine();
//                            while (responseData != null)
//                            {
//                                responseData += reader.ReadLine();
//                            }
//                        }
//                        return responseData;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        [TestMethod]
//        public void ThreeDCube()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/3d-cube.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void ThreeDMorph()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/3d-morph.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void ThreeDRaytrace()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/3d-raytrace.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void AccessBinaryTrees()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/access-binary-trees.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void AccessFannkych()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/access-fannkuch.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void AccessNBody()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/access-nbody.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void AccessNSieve()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/access-nsieve.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void Bitops3BitBitsInByte()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/bitops-3bit-bits-in-byte.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void BitopsBitsInByte()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/bitops-bits-in-byte.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void BitopsBitwiseAnd()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/bitops-bitwise-and.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void BitopsNSieveBits()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/bitops-nsieve-bits.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void ControlFlowRecursive()
//        {
//            var t = new Thread(() =>
//            {
//                var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/controlflow-recursive.js"); // new WebClient().DownloadString(url);
//                RunTest(content);
//            }, 1000000000);

//            t.Start();
//            t.Join();
//        }

//        [TestMethod]
//        public void CryptoAES()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/crypto-aes.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void CryptoMD5()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/crypto-md5.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void CryptoSha1()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/crypto-sha1.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void DateFormatTofte()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/date-format-tofte.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void DateFormatXParb()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/date-format-xparb.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void MathCordic()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/math-cordic.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void MathPartialSums()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/math-partial-sums.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void MathSpectralNorm()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/math-spectral-norm.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void RegexpDna()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/regexp-dna.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void StringBase64()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/string-base64.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void StringFasta()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/string-fasta.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void StringTagCloud()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/string-tagcloud.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void StringUnpackCode()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/string-unpack-code.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }

//        [TestMethod]
//        public void StringValidateInput()
//        {
//            var content = GetJavaScript("https://raw.github.com/WebKit/webkit/master/PerformanceTests/SunSpider/tests/sunspider-1.0.1/string-validate-input.js"); // new WebClient().DownloadString(url);
//            RunTest(content);
//        }
//    }
//}
