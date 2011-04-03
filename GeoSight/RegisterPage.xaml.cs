using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoSight
{
    public partial class RegisterPage : PhoneApplicationPage
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
        public RegisterPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
            responseDelegate = new EventDelegates.HTTPResponseDelegate(ProcessRegisterRequest);
            failDelegate = new EventDelegates.HTTPFailDelegate(FailRegisterRequest);
        }

        /// <summary>
        /// Called when the register HTTP request to the server was successful.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        private void ProcessRegisterRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);

            try
            {
                // Try to parse the HTTP response as a JSON object
                JObject registerInfo = JObject.Parse(reader.ReadToEnd());

                // If the HTTP response parsed as JSON, we were able to register.
                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() => { this.NavigationService.GoBack(); }));
            }
            catch (JsonReaderException e)
            {
                // If the HTTP response did NOT parse as JSON, registering failed.
                NotificationTextBlock.Dispatcher.BeginInvoke(
                    new Action(() => { NotificationTextBlock.Text = "Register Failed."; }));
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Called when the register HTTP request to the server failed.
        /// </summary>
        /// <param name="message">A message that contains the reason
        /// for the failure.</param>
        private void FailRegisterRequest(String message)
        {
            NotificationTextBlock.Dispatcher.BeginInvoke(
                new Action(() => { NotificationTextBlock.Text = "Register Failed: " + message + "."; }));
        }

        /// <summary>
        /// Called when the "Register" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Check that the user entered their first name
            String firstName = FirstNameTextBox.Text;
            if (firstName == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter your first name.";
                return;
            }

            // Check that the user entered their last name
            String lastName = LastNameTextBox.Text;
            if (lastName == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter your last name.";
                return;
            }

            // Check that the user entered an email address
            String emailAddress = EmailAddressTextBox.Text;
            if (emailAddress == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter your email address.";
                return;
            }

            // Check that the user entered a password
            String password = PasswordTextBox.Password;
            if (password == String.Empty)
            {
                NotificationTextBlock.Text = "Please enter your password.";
                return;
            }

            // Check that the user comfirmed the password
            String passwordConfirm = PasswordConfirmTextBox.Password;
            if (passwordConfirm == String.Empty || password != passwordConfirm)
            {
                NotificationTextBlock.Text = "Please confirm your password.";
                return;
            }

            // Clear the error message text block
            NotificationTextBlock.Text = String.Empty;

            // Send register request to server
            App.ServerConnection.Register(
                firstName,
                lastName,
                emailAddress,
                password,
                passwordConfirm,
                responseDelegate,
                failDelegate);
        }
    }
}