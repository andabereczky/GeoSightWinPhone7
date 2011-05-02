﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace GeoSight
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// The task used to capture a picture.
        /// </summary>
        CameraCaptureTask takeAPhotoTask;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Initialize the camera task
            takeAPhotoTask = new CameraCaptureTask();
            takeAPhotoTask.Completed += new EventHandler<PhotoResult>(TakeAPhotoTask_Completed);

            // Initialize the GPS location
            new GPSLocation();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            // Check if we're logged in.
            if (App.LoginFirstName != String.Empty)
            {
                LoginMessageTextBlock.Text = "Logged in as " + App.LoginFirstName;
            }
            else
            {
                LoginMessageTextBlock.Text = "Not logged in.";
            }
        }

        /*
        private void DownloadSelectedSightPicture()
        {
            App.ServerConnection.DownloadPicture(
                App.SelectedSight.ThumbnailURL,
                new EventDelegates.HTTPResponseDelegate(ProcessDownloadRequest),
                new EventDelegates.HTTPFailDelegate(FailDownloadRequest));
        }
        
        private void ProcessDownloadRequest(Stream responseStream)
        {
            Debug.WriteLine("Photo download succeeded!");

            // Read image bytes from HTTP response.
            byte[] contents;
            using (BinaryReader bReader = new BinaryReader(responseStream))
            {
                contents = bReader.ReadBytes((int)responseStream.Length);
            }

            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    // Save image to isolated storage.
                    String tempJPEG = "TempJPEG";
                    var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                    if (myStore.FileExists(tempJPEG))
                    {
                        myStore.DeleteFile(tempJPEG);
                    }
                    IsolatedStorageFileStream stream = myStore.CreateFile(tempJPEG);
                    stream.Write(contents, 0, contents.Length);
                    stream.Close();

                    // Display image.
                    stream = new IsolatedStorageFileStream(tempJPEG, FileMode.Open, myStore);
                    BitmapImage image = new BitmapImage();
                    image.SetSource(stream);
                    image1.Source = image;
                }));
        }

        private void FailDownloadRequest(String message)
        {
            Debug.WriteLine("Photo download failed:\n" + message);
        }
        */

        private void TakePhotoButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            // Make sure the user is logged in.
            if (App.LoginFirstName == String.Empty)
            {
                MessageBox.Show("Please log in.");
                return;
            }

            // Make sure that the user has chosen a sight.
            if (App.SelectedSight == null)
            {
                MessageBox.Show("Please pick a sight.");
                return;
            }

            // Make sure that the user has navigated to the chosen sight.
            if (!App.InDestination)
            {
                MessageBox.Show("Please go to the chosen sight.");
                return;
            }

            takeAPhotoTask.Show();
        }

        /// <summary>
        /// Event handler for retrieving the JPEG photo stream.
        /// Also to for decoding JPEG stream into a writeable bitmap and displaying.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void TakeAPhotoTask_Completed(object sender, PhotoResult eventArgs)
        {

            if (eventArgs.TaskResult == TaskResult.OK && eventArgs.ChosenPhoto != null)
            {
                // Save the captured image to disk.
                Picture pic = SaveCapturedImage(eventArgs.ChosenPhoto);

                // Upload the image to the server.
                UploadPhoto(pic);
            }
        }

        private Picture SaveCapturedImage(Stream imageSource)
        {
            Stream stream;
            bool useFile = true;

            //Take JPEG stream and decode into a WriteableBitmap object.
            WriteableBitmap wb = PictureDecoder.DecodeJpeg(imageSource);

            if (useFile)
            {

                // Create a filename for JPEG file in isolated storage.
                String tempJPEG = "TempJPEG";

                // Create virtual store and file stream. Check for duplicate tempJPEG files.
                var myStore = IsolatedStorageFile.GetUserStoreForApplication();
                if (myStore.FileExists(tempJPEG))
                {
                    myStore.DeleteFile(tempJPEG);
                }

                IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);

                // Encode WriteableBitmap object to a JPEG stream.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                myFileStream.Close();

                // Create a new stream from isolated storage.
                stream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);
            } else {

                // Create a byte stream from the most-recently captured bitmap.
                byte[] bytes = ToByteArray(wb);
                stream = new MemoryStream(bytes);
            }

            // Save the JPEG file to the media library on Windows Phone.
            MediaLibrary library = new MediaLibrary();
            Picture pic = library.SavePicture(App.ImageFilename, stream);
            stream.Close();
            return pic;
        }

        private static byte[] ToByteArray(WriteableBitmap bmp) {
            int[] p = bmp.Pixels;
            int len = p.Length * 4;
            byte[] result = new byte[len];
            Buffer.BlockCopy(p, 0, result, 0, len);
            return result;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            this.NavigationService.Navigate(new Uri("/RegisterPage.xaml", UriKind.Relative));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        private void PickASightButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            App.SelectedSight = null;
            this.NavigationService.Navigate(new Uri("/PickSightPage.xaml", UriKind.Relative));
        }

        private void ProcessUploadRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);
            string line;
            Debug.WriteLine("Upload succeeded:\n");
            while ((line = reader.ReadLine()) != null)
            {
                Debug.WriteLine(line);
            }
            reader.Close();
        }

        private void FailUploadRequest(String message)
        {
            Debug.WriteLine("Upload failed:\n" + message);
        }

        private void UploadPhoto(Picture picture)
        {
            // Load the photo taken with the camera into memory.
            Stream imageStream = picture.GetImage();
            int imageSize = (int)imageStream.Length;
            BinaryReader binReader = new BinaryReader(imageStream);
            byte[] imageBytes = new byte[imageSize];
            int count = binReader.Read(imageBytes, 0, (int)imageSize);
            binReader.Close();

            // Upload the image to the server.
            App.ServerConnection.UploadPhoto(
                (int) App.LoginUserID,
                imageBytes,
                new EventDelegates.HTTPResponseDelegate(ProcessUploadRequest),
                new EventDelegates.HTTPFailDelegate(FailUploadRequest));
        }
    }
}
