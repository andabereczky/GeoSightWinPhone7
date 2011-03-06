using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace GeoSight
{
    public partial class LoginPage : PhoneApplicationPage
    {


        /// <summary>
        /// The client used to send HTTP requests.
        /// </summary>
        WebClient webClient;

        EventDelegates.HTTPResponseDelegate responseDelegate;

        EventDelegates.HTTPFailDelegate failDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
            webClient = new WebClient();
            responseDelegate = new EventDelegates.HTTPResponseDelegate(processLoginRequest);
            failDelegate = new EventDelegates.HTTPFailDelegate(failLoginRequest);
        }

        private void processLoginRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);

            try
            {
                JObject loginInfo = JObject.Parse(reader.ReadToEnd());
                IEnumerator enum1 = loginInfo.GetEnumerator();
                while (enum1.MoveNext())
                {
                    Debug.WriteLine(enum1.Current.ToString());
                }

                // If the HTTP response parsed as JSON, we are logged in.
                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() => { App.LoggedIn = true; this.NavigationService.GoBack(); }));
            }
            catch (JsonReaderException e)
            {
                // If the HTTP response did NOT parse as JSON, login failed.
                NotificationTextBlock.Dispatcher.BeginInvoke(
                    new Action(() => { NotificationTextBlock.Text = "Login Failed: Invalid email address and/or password."; }));
            }
            catch (UnauthorizedAccessException e2)
            {
                Debug.WriteLine(e2.StackTrace);
            }
            finally
            {
                reader.Close();
            }
        }

        private void failLoginRequest(String message)
        {
            NotificationTextBlock.Dispatcher.BeginInvoke(
                new Action(() => { NotificationTextBlock.Text = "Login Failed: " + message + "."; }));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Check that the user entered an email address
            String emailAddress = EmailAddressTextBox.Text;
            if (emailAddress == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter an email address.";
                return;
            }

            // Check that the user entered a password
            String password = PasswordTextBox.Password;
            if (password == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter a password.";
                return;
            }

            // Clear the error message text block
            NotificationTextBlock.Text = String.Empty;

            // Build a dictionary which contains the email address and password.
            // This dictionary will be used to create the POST variables.
            Dictionary<String, String> vars = new Dictionary<String, String>();
            vars.Add("user_session[email]", emailAddress);
            vars.Add("user_session[password]", password);

            webClient.SendReqest(
                true,
                App.serverURL + App.loginURL,
                vars,
                responseDelegate,
                failDelegate);
        }
    }
}