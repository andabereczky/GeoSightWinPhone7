using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace GeoSight
{
    public partial class PickSightMapPage : PhoneApplicationPage
    {
        /// <summary>
        /// The client used to send HTTP requests.
        /// </summary>
        WebClient webClient;

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
        public PickSightMapPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
            webClient = new WebClient();
            responseDelegate = new EventDelegates.HTTPResponseDelegate(ProcessSightsListRequest);
            failDelegate = new EventDelegates.HTTPFailDelegate(FailSightsListRequest);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Ask the user to wait until the sights list has been downloaded.
            NotificationTextBlock.Text = "Please wait...";

            // Get the list of sights from the server.
            GetSightsList();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            // this is called when you close the page.
        }

        /// <summary>
        /// Send a message to the web server, requesting the current list of sights.
        /// </summary>
        private void GetSightsList()
        {
            webClient.SendReqest(
                false,
                App.serverURL + App.sightsListURL,
                new Dictionary<String, String>(),
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Called when the HTTP message to the server requesting the list of
        /// sights was successful.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        private void ProcessSightsListRequest(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);

            try
            {
                // Try to parse the HTTP response as a JSON object
                JArray jsonSights = JArray.Parse(reader.ReadToEnd());

                // Created a collection of sights sorted by the distance to the current location
                Sights sights = new Sights(jsonSights);

                // TODO: show the sights on the map
            }
            catch (JsonReaderException e)
            {
                // If the HTTP response did NOT parse as JSON, getting the list of sights failed.
                NotificationTextBlock.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        NotificationTextBlock.Text = "Retrieving Sights Failed: Unknown error.";
                    }));
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Called when the HTTP message to the server requesting the list of
        /// sights failed.
        /// </summary>
        /// <param name="message">A message that contains the reason
        /// for the failure.</param>
        private void FailSightsListRequest(String message)
        {
            NotificationTextBlock.Dispatcher.BeginInvoke(
                new Action(() => { NotificationTextBlock.Text = "Retrieving Sights Failed: " + message + "."; }));
        }
    }
}