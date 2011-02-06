using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace GeoSight
{
    public partial class MainPage : PhoneApplicationPage
    {
        // The camera used to capture a picture.
        CameraCaptureTask ctask;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            ctask = new CameraCaptureTask();

            // Create new event handler for capturing a photo.
            ctask.Completed += new EventHandler<PhotoResult>(ctask_Completed);
        }

        private void takePhotoButton_Click(object sender, RoutedEventArgs e)
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
                MainImage.Source = App.CapturedImage;
            }
        }

        private void savePhotoButton_Click(object sender, RoutedEventArgs e)
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
                Extensions.SaveJpeg(wb, myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
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

        public static byte[] ToByteArray(WriteableBitmap bmp) {
            int[] p = bmp.Pixels;
            int len = p.Length * 4;
            byte[] result = new byte[len];
            Buffer.BlockCopy(p, 0, result, 0, len);
            return result;
        }
    }
}