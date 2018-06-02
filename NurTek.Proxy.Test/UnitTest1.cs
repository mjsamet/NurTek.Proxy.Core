using System;
using System.Collections.Specialized;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NurTek.Proxy.Core.Http;

namespace NurTek.Proxy.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGoogle()
        {
            Request request = new Request(HttpMethod.Get, "http://www.google.com.tr", new NameValueCollection(), new NameValueCollection());
            Core.Proxy proxy = new Core.Proxy();
            var response = proxy.Forward(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(!string.IsNullOrEmpty(response.Content));
        }

        [TestMethod]
        public void TestStream()
        {
            Request request = new Request(HttpMethod.Get, "https://www.google.com.tr/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png", new NameValueCollection(), new NameValueCollection());
            Core.Proxy proxy = new Core.Proxy();
            var response = proxy.Forward(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsStream);
        }
    }
}
