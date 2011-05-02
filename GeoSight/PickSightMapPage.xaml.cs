using System;
using System.Device.Location;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoSight
{
    public partial class PickSightMapPage : PhoneApplicationPage
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
        /// The sights to be shown in the map.
        /// </summary>
        Sights sights;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PickSightMapPage()
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

                // Created a collection of sights 
                sights = new Sights(jsonSights, App.CurrentLatitude, App.CurrentLongitude);

                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        // For each sight, place a pin on the map.
                        foreach (Sight sight in sights)
                        {
                            var pin = new Pushpin();
                            pin.Location = new GeoCoordinate(sight.Latitude, sight.Longitude);
                            pin.Content = sight.Name;
                            sightsMap.Children.Add(pin);

                            // Add a handler for when pin is clicked, select that sight.
                            pin.AddHandler(Pushpin.MouseLeftButtonUpEvent, new MouseButtonEventHandler(PinClick), true);
                        }
                        NotificationTextBlock.Text = "";
                    })
                );
                

                // Show the map and buttons for map on the page.
                Deployment.Current.Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       sightsMap.Visibility = Visibility.Visible;
                       ZoomInButton.Visibility = Visibility.Visible;
                       ZoomOutButton.Visibility = Visibility.Visible;
                       ChangeToRoadViewButton.Visibility = Visibility.Visible;
                       ChangeToAerialViewButton.Visibility = Visibility.Visible;

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

                //change so that current location is on center?
                NotificationTextBlock.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        sightsMap.Center = new GeoCoordinate(App.CurrentLatitude, App.CurrentLongitude);
                        sightsMap.ZoomLevel = 8;
                    })
                );

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

        /// <summary>
        /// Called for left button up on mouse for the pin.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void PinClick(object sender, MouseButtonEventArgs eventArgs)
        {
            Pushpin pin = (Pushpin) sender;
            Sight selection = null;

            // Find the appropriate sight from the list of sights with the pin's coordinate.
            foreach (Sight sight in sights)
            {
                if (pin.Location.Latitude == sight.Latitude & pin.Location.Longitude == sight.Longitude)
                {
                    selection = sight;
                }
            }

            // Select that sight and go back to main page.
            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    App.SelectedSight = selection;
                    this.NavigationService.GoBack();

                })
            );
        }

        /// <summary>
        /// Zoom in to the map on the center of the map element.
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// </summary>
        private void ZoomIn(object sender, RoutedEventArgs eventArgs)
        {
            sightsMap.ZoomLevel = sightsMap.ZoomLevel + 1;
        }

        /// <summary>
        /// Zoom out from the map on the center of the map element.
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// </summary>
        private void ZoomOut(object sender, RoutedEventArgs eventArgs)
        {
            sightsMap.ZoomLevel = sightsMap.ZoomLevel - 1;
        }

        /// <summary>
        /// Changes the map view to Road mode.
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// </summary>
        private void ChangeToRoadView(object sender, RoutedEventArgs eventArgs)
        {
            sightsMap.Mode = new RoadMode();
        }

        /// <summary>
        /// Changes the map view to Aerial mode.
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// </summary>
        private void ChangeToAerialView(object sender, RoutedEventArgs eventArgs)
        {
            sightsMap.Mode = new AerialMode();
        }
    }
}
