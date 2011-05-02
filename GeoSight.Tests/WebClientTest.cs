using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoSight.Tests
{
    [TestClass]
    public class WebClientTest
    {
        [TestMethod]
        public void TestSendRequest()
        {
            new WebClient("http").SendRequest(
                false,
                "www.google.com",
                new Dictionary<String, String>(),
                new EventDelegates.HTTPResponseDelegate(ProcessResponse),
                new EventDelegates.HTTPFailDelegate(FailRequest));

            new WebClient("http").SendRequest(
                false,
                "www.jdjhksd.com",
                new Dictionary<String, String>(),
                new EventDelegates.HTTPResponseDelegate(ProcessResponse2),
                new EventDelegates.HTTPFailDelegate(FailRequest2));
        }

        private void ProcessResponse(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);
            string response = reader.ReadToEnd();
            Debug.WriteLine(response);
            Assert.IsTrue(response.Length > 0);
            reader.Close();
        }

        private void FailRequest(String message)
        {
            Assert.IsTrue(false);
        }

        private void ProcessResponse2(Stream responseStream)
        {
            Assert.IsTrue(false);
        }

        private void FailRequest2(String message)
        {
            Assert.IsTrue(true);
        }
    }
}
