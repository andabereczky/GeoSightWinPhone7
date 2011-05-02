using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GeoSight
{
    /// <summary>
    /// A class used to connect to the server and complete various tasks.
    /// </summary>
    public class ServerConnection
    {
        #region Private static member variables

        private static String serverURL = "geosight.heroku.com";

        private static String registerURL = "/users.json";

        private static String loginURL = "/user_sessions.json";

        private static String sightsListURL = "/sights.json";

        private static String uploadURL = "/photos";

        #endregion

        #region Private member variables

        /// <summary>
        /// The web client object used to send requests to the server.
        /// </summary>
        private WebClient webClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor. Creates a new web client, capable of sending requests
        /// to the server using https.
        /// </summary>
        public ServerConnection()
        {
            this.webClient = new WebClient("https");
        }

        #endregion

        # region Public methods

        /// <summary>
        /// Send a message to the web server, requesting to login.
        /// </summary>
        /// <param name="emailAddress">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void Login(
            String emailAddress,
            String password,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Build a dictionary which contains the email address and password.
            // This dictionary will be used to create the POST variables.
            Dictionary<String, String> vars = new Dictionary<String, String>();
            vars.Add("user_session[email]", emailAddress);
            vars.Add("user_session[password]", password);

            this.webClient.SendRequest(
                true,
                serverURL + loginURL,
                vars,
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Send a message to the web server, requesting the current list of sights.
        /// </summary>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void GetSightsList(
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            this.webClient.SendRequest(
                false,
                serverURL + sightsListURL,
                new Dictionary<String, String>(),
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Send a message to the web server, requesting to register a new user.
        /// </summary>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <param name="emailAddress">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="passwordConfirm">The user's password again.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void Register(
            string firstName,
            string lastName,
            string emailAddress,
            string password,
            string passwordConfirm,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Build a dictionary which contains the user's first name,
            // last name, email address, and password.
            // This dictionary will be used to create the POST variables.
            Dictionary<String, String> vars = new Dictionary<String, String>();
            vars.Add("user[first_name]", firstName);
            vars.Add("user[last_name]", lastName);
            vars.Add("user[email]", emailAddress);
            vars.Add("user[password]", password);
            vars.Add("user[password_confirmation]", passwordConfirm);

            this.webClient.SendRequest(
                true,
                serverURL + registerURL,
                vars,
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Send a message to the web server, uploading a new picture.
        /// </summary>
        /// <param name="userID">The user's ID.</param>
        /// <param name="imageBytes">The picture to be uploaded as a byte array.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void UploadPhoto(
            int userID,
            byte[] imageBytes,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Build a dictionary which contains the user ID.
            // This dictionary will be used to create the POST variables.
            Dictionary<String, String> vars = new Dictionary<String, String>();
            vars.Add("photo[user_id]", userID.ToString());
            vars.Add("photo[latitude]", Convert.ToString(App.CurrentLatitude));
            vars.Add("photo[longitude]", Convert.ToString(App.CurrentLongitude));

            // Create the file data needed for the upload.
            String fileName = App.ImageFilename;
            String varName = "photo[file]";

            // Upload the photo.
            this.webClient.UploadFile(
                serverURL + uploadURL,
                fileName,
                varName,
                imageBytes,
                vars,
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Send a message to Amazon S3, requesting a picture.
        /// </summary>
        /// <param name="url">The picture URL.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void DownloadPicture(
            String url,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Remove the "https://" from the beginning of the string.
            if (url.StartsWith("https://"))
                url = url.Substring(9);

            this.webClient.SendRequest(
                false,
                url.Substring(9),
                new Dictionary<String, String>(),
                responseDelegate,
                failDelegate);
        }

        #endregion
    }
}
