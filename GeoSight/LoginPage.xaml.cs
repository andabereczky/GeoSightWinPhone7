using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoSight
{
    public partial class LoginPage : PhoneApplicationPage
    {
        /// <summary>
        /// A delegate used as a callback when an HTTP response is received.
        /// </summary>
        EventDelegates.HTTPResponseDelegate responseDelegate;

        /// <summary>
        /// A delegate used as a callback when an HTTP request fails.
        /// </summary>
        EventDelegates.HTTPFailDelegate failDelegate;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
            responseDelegate = new EventDelegates.HTTPResponseDelegate(ProcessLoginRequest);
            failDelegate = new EventDelegates.HTTPFailDelegate(FailLoginRequest);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.LoggedIn)
            {
                NotificationTextBlock.Text = "Already logged in.";
            }
            else
            {
                NotificationTextBlock.Text = String.Empty;
            }
        }

        /// <summary>
        /// Called when the login HTTP request to the server was successful.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        private void ProcessLoginRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);

            try
            {
                // Try to parse the HTTP response as a JSON object
                JObject loginInfo = JObject.Parse(reader.ReadToEnd());

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
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Called when the login HTTP request to the server failed.
        /// </summary>
        /// <param name="message">A message that contains the reason
        /// for the failure.</param>
        private void FailLoginRequest(String message)
        {
            NotificationTextBlock.Dispatcher.BeginInvoke(
                new Action(() => { NotificationTextBlock.Text = "Login Failed: " + message + "."; }));
        }

        /// <summary>
        /// Called when the "Login" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // Send login request to server
            App.ServerConnection.Login(
                emailAddress,
                password,
                responseDelegate,
                failDelegate);
        }
    }
}
