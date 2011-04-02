using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace GeoSight.Tests
{
    [TestClass]
    public class SightsTest
    {
        [TestMethod]
        public void TestSights()
        {
            // Read sights in JSON format from text file
            Uri uri = new Uri("sights.txt", UriKind.Relative);
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
            Stream stream = streamResourceInfo.Stream;
            StreamReader reader = new StreamReader(stream);

            // Parse JSON
            JArray jsonSights = JArray.Parse(reader.ReadToEnd());

            // Create sights
            Sights sights = new Sights(jsonSights);

            // Check that the sights were created correctly
            Assert.AreEqual(2, sights.Count);
            Assert.AreEqual("\"well\"", sights[0].Name);
            Assert.AreEqual("\"Das Cafe\"", sights[1].Name);

            // Check that the sights are sorted correctly
            ObservableCollection<Sight> sortedSights =
                Sights.GetSortedSights(sights, 40.115627, -88.220987);
            Assert.AreEqual(sights.Count, sortedSights.Count);
            Assert.AreEqual("\"Das Cafe\"", sortedSights[0].Name);
            Assert.AreEqual("\"well\"", sortedSights[1].Name);
        }
    }
}
