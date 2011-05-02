using System;
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
            // Initialize the page
            InitializeComponent();

            // Initialize the camera task
            takeAPhotoTask = new CameraCaptureTask();
            takeAPhotoTask.Completed += new EventHandler<PhotoResult>(TookAPhoto);

            // Initialize the GPS location
            new GPSLocation();
        }

        /// <summary>
        /// Converts a bitmap to a byte array.
        /// </summary>
        /// <param name="bmp">A bitmap.</param>
        /// <returns>A byte array.</returns>
        private static byte[] ToByteArray(WriteableBitmap bmp)
        {
            int[] p = bmp.Pixels;
            int len = p.Length * 4;
            byte[] result = new byte[len];
            Buffer.BlockCopy(p, 0, result, 0, len);
            return result;
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
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

        /// <summary>
        /// Called when the "Login" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ShowLoginPage(object sender, RoutedEventArgs eventArgs)
        {
            this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Called when the "Register" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ShowRegisterPage(object sender, RoutedEventArgs eventArgs)
        {
            this.NavigationService.Navigate(new Uri("/RegisterPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Called when the "Pick a Sight" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ShowPickASightPage(object sender, RoutedEventArgs eventArgs)
        {
            App.SelectedSight = null;
            this.NavigationService.Navigate(new Uri("/PickSightPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Called when the "Take a Photo" button is clicked.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void ValidateTakePhotoInput(object sender, RoutedEventArgs eventArgs)
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
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void TookAPhoto(object sender, PhotoResult eventArgs)
        {

            if (eventArgs.TaskResult == TaskResult.OK && eventArgs.ChosenPhoto != null)
            {
                // Save the captured image to disk.
                Picture pic = SaveCapturedImage(eventArgs.ChosenPhoto);

                // Upload the image to the server.
                UploadPhoto(pic);
            }
        }

        /// <summary>
        /// Save the photo that was taken to disk.
        /// </summary>
        /// <param name="imageSource">The stream containing the photo that was taken.</param>
        /// <returns>A Media Library picture object of the saved photo.</returns>
        private Picture SaveCapturedImage(Stream imageSource)
        {
            // Local variables.
            Stream stream;

            //Take JPEG stream and decode into a WriteableBitmap object.
            WriteableBitmap wb = PictureDecoder.DecodeJpeg(imageSource);

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

            // Save the JPEG file to the media library on Windows Phone.
            MediaLibrary library = new MediaLibrary();
            Picture pic = library.SavePicture(App.ImageFilename, stream);
            stream.Close();
            return pic;
        }

        /// <summary>
        /// Called when the upload HTTP request to the server was successful.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        private void UploadSucceeded(Stream responseStream)
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

        /// <summary>
        /// Called when the upload HTTP request to the server failed.
        /// </summary>
        /// <param name="message">A message that contains the reason
        /// for the failure.</param>
        private void UploadFailed(String message)
        {
            Debug.WriteLine("Upload failed:\n" + message);
        }

        /// <summary>
        /// Uploads the given picture to the server.
        /// </summary>
        /// <param name="picture">A Media Library picture.</param>
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
                new EventDelegates.HTTPResponseDelegate(UploadSucceeded),
                new EventDelegates.HTTPFailDelegate(UploadFailed));
        }
    }
}
