using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoSight.Tests
{
    [TestClass]
    public class GPSLocationTest
    {
        [TestMethod]
        public void TestGPSLocation()
        {
            GPSLocation location = new GPSLocation();
            Assert.IsTrue(location.Started);
        }
    }
}
