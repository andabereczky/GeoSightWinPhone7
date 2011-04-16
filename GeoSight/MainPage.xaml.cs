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
        /// The camera used to capture a picture.
        /// </summary>
        CameraCaptureTask ctask;

        /// <summary>
        /// Supplies location data that is based on latitude and longitude.
        /// </summary>
        GPSLocation location;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Initialize the camera task
            ctask = new CameraCaptureTask();
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);

            // Initialize the GPS location
            location = new GPSLocation();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
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

            // Check if a sight has been selected.
            if (App.SelectedSight != null)
            {
                PickSightMessageTextBlock.Text = "Selected " + App.SelectedSight.Name;
            }
            else
            {
                PickSightMessageTextBlock.Text = "No sight selected.";
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
                // Save the captured image to disk.
                SaveCapturedImage(e.ChosenPhoto);
            }
        }

        private void SaveCapturedImage(Stream imageSource)
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
        }

        private static byte[] ToByteArray(WriteableBitmap bmp) {
            int[] p = bmp.Pixels;
            int len = p.Length * 4;
            byte[] result = new byte[len];
            Buffer.BlockCopy(p, 0, result, 0, len);
            return result;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/RegisterPage.xaml", UriKind.Relative));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        private void PickASightButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightPage.xaml", UriKind.Relative));
        }

        private void btn_PickSightFromMap_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/PickSightMapPage.xaml", UriKind.Relative));
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

        private void UploadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the user is logged in.
            if (App.LoginFirstName == String.Empty)
            {
                MessageBox.Show("Please log in.");
                return;
            }

            // Find the photo in the media library.
            MediaLibrary library = new MediaLibrary();
            Picture picture = null;
            foreach (Picture pic in library.Pictures)
            {
                if (pic.Name == App.ImageFilename)
                {
                    picture = pic;
                    break;
                }
            }
            if (picture == null)
            {
                MessageBox.Show("Please take a picture.");
                return;
            }

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
