using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoSight
{
    public partial class PickSightListPage : PhoneApplicationPage
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
        public PickSightListPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
            responseDelegate = new EventDelegates.HTTPResponseDelegate(GettingSightsListSucceeded);
            failDelegate = new EventDelegates.HTTPFailDelegate(GettingSightsListFailed);
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            // Ask the user to wait until the sights list has been downloaded.
            NotificationTextBlock.Text = "Please wait...";

            // Get the list of sights from the server.
            GetSightsList();
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            // Make sure no item is highlighted in the list of sights.
            SightsList.SelectedIndex = -1;
            SightsList.SelectedItem = null;
        }

        /// <summary>
        /// Event handler called when user selects a sight.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void NewSightSelected(object sender, SelectionChangedEventArgs eventArgs)
        {
            // If an item was selected...
            if (SightsList.SelectedIndex != -1)
            {
                // Get the currently selected sight.
                Sight sight = (Sight) SightsList.SelectedItem;

                // Save the selected sight and navigate back to the main page.
                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        App.SelectedSight = sight;
                        this.NavigationService.GoBack();
                    }));
            }
        }

        /// <summary>
        /// Send a message to the web server, requesting the current list of sights.
        /// </summary>
        private void GetSightsList()
        {
            App.ServerConnection.GetSightsList(
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Called when the HTTP message to the server requesting the list of
        /// sights was successful.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        private void GettingSightsListSucceeded(Stream responseStream)
        {
            StreamReader reader = new StreamReader(responseStream);

            try
            {
                // Try to parse the HTTP response as a JSON object
                JArray jsonSights = JArray.Parse(reader.ReadToEnd());

                // Created a collection of sights sorted by the distance to the current location
                Sights sights = new Sights(jsonSights, App.CurrentLatitude, App.CurrentLongitude);
                ObservableCollection<Sight> sortedSights = Sights.GetSortedSights(sights);

                // If the HTTP response parsed as JSON, we got the list of sights.
                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        SightsList.ItemsSource = sortedSights;
                        NotificationTextBlock.Text = "";
                    }));
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
        private void GettingSightsListFailed(String message)
        {
            NotificationTextBlock.Dispatcher.BeginInvoke(
                new Action(() => { NotificationTextBlock.Text = "Retrieving Sights Failed: " + message + "."; }));
        }
    }
}
