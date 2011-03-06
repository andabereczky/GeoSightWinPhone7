using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Windows.Navigation;

namespace GeoSight
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// The client used to send HTTP requests.
        /// </summary>
        WebClient webClient;

        /// <summary>
        /// The camera used to capture a picture.
        /// </summary>
        CameraCaptureTask ctask;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            webClient = new WebClient();
            ctask = new CameraCaptureTask();
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.LoggedIn)
            {
                LoginMessageTextBlock.Text = "Logged in.";
            }
            else
            {
                LoginMessageTextBlock.Text = "Not logged in.";
            }
        }

        private void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            ctask.Show();
        }

        /// <summary>
        /// Event handler for retrieving the JPEG photo stream.
        /// Also to for decoding JPEG stream into a writeable bitmap and displaying.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ctask_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {

                //Take JPEG stream and decode into a WriteableBitmap object.
                App.CapturedImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);

                //Populate image control with WriteableBitmap object.
                img_MainImage.Source = App.CapturedImage;
            }
        }

        private void SavePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Stream stream;
            bool useFile = true;

            if (useFile) {

                // Create a filename for JPEG file in isolated storage.
                String tempJPEG = "TempJPEG";

                // Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(tempJPEG))
                {
                    myStore.DeleteFile(tempJPEG);
                }

                IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);

                // Get the most-recently captured bitmap.
                WriteableBitmap wb = App.CapturedImage;

                // Encode WriteableBitmap object to a JPEG stream.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                myFileStream.Close();

                // Create a new stream from isolated storage.
                stream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);
            } else {

                // Create a byte stream from the most-recently captured bitmap.
                byte[] bytes = ToByteArray(App.CapturedImage);
                stream = new MemoryStream(bytes);
            }

            // Save the JPEG file to the media library on Windows Phone.
            MediaLibrary library = new MediaLibrary();
            Picture pic = library.SavePicture("SavedPicture.jpg", stream);
            stream.Close();
        }

        private static byte[] ToByteArray(WriteableBitmap bmp) {
            int[] p = bmp.Pixels;
            int len = p.Length * 4;
            byte[] result = new byte[len];
            Buffer.BlockCopy(p, 0, result, 0, len);
            return result;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        private void processSightsListRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);
            JArray array = JArray.Parse(reader.ReadToEnd());
            reader.Close();

            for (int i = 0; i <= array.Count - 1; i++)
            {
                Debug.WriteLine(array[i]["sight"]["name"]);
                Debug.WriteLine(array[i]["sight"]["radius"]);
                Debug.WriteLine(array[i]["sight"]["created_at"]);
                Debug.WriteLine(array[i]["sight"]["latitude"]);
                Debug.WriteLine(array[i]["sight"]["updated_at"]);
                Debug.WriteLine(array[i]["sight"]["id"]);
                Debug.WriteLine(array[i]["sight"]["user_id"]);
                Debug.WriteLine(array[i]["sight"]["longitude"]);

            }
        }

        private void failSightsListRequest(String message)
        {
            Debug.WriteLine("Retrieving the sights list failed with the following message:");
            Debug.WriteLine(message);
        }

        private void PickASightButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, String> vars = new Dictionary<String, String>();

            EventDelegates.HTTPResponseDelegate responseDelegate =
                new EventDelegates.HTTPResponseDelegate(processSightsListRequest);
            EventDelegates.HTTPFailDelegate failDelegate =
                new EventDelegates.HTTPFailDelegate(failSightsListRequest);

            webClient.SendReqest(
                false,
                App.serverURL + App.sightsListURL,
                vars,
                responseDelegate,
                failDelegate);
        }
    }
}