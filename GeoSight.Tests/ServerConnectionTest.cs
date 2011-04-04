using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoSight.Tests
{
    [TestClass]
    public class ServerConnectionTest
    {
        [TestMethod]
        public void TestServerConnection()
        {
            ServerConnection sc = new ServerConnection();

            // User information.
            //string firstName = "Harry";
            //string lastName = "Potter";
            string emailAddress = "harry.potter@gmail.com";
            string password = "1234";

            //sc.Register(
            //    firstName,
            //    lastName,
            //    emailAddress,
            //    password,
            //    password,
            //    new EventDelegates.HTTPResponseDelegate(ProcessResponse),
            //    new EventDelegates.HTTPFailDelegate(FailRequest));

            sc.Login(
                emailAddress,
                password,
                new EventDelegates.HTTPResponseDelegate(ProcessResponse),
                new EventDelegates.HTTPFailDelegate(FailRequest));

            sc.GetSightsList(
                new EventDelegates.HTTPResponseDelegate(ProcessResponse),
                new EventDelegates.HTTPFailDelegate(FailRequest));

            // Load photo from file.
            Uri uri = new Uri("img.jpg", UriKind.Relative);
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
            Stream imageStream = streamResourceInfo.Stream;
            int imageSize = (int)imageStream.Length;
            BinaryReader binReader = new BinaryReader(imageStream);
            byte[] imageBytes = new byte[imageSize];
            int count = binReader.Read(imageBytes, 0, (int)imageSize);
            binReader.Close();

            sc.UploadPhoto(
                8, // Harry Potter's user ID
                imageBytes,
                new EventDelegates.HTTPResponseDelegate(ProcessResponse),
                new EventDelegates.HTTPFailDelegate(FailRequest));
        }

        void ProcessResponse(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);
            string response = reader.ReadToEnd();
            Debug.WriteLine(response);
            Assert.IsTrue(response.Length > 0);
            reader.Close();
        }

        void FailRequest(String message)
        {
            Assert.IsTrue(false);
        }
    }
}
