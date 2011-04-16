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
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using System.Windows.Input;

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
        /// Constructor.
        /// </summary>
        public PickSightMapPage()
        {
            // Initialize the page
            InitializeComponent();

            // Initialize members variables
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
            App.ServerConnection.GetSightsList(
                responseDelegate,
                failDelegate);
        }

        Sights sights;

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

                // Created a collection of sights 
                sights = new Sights(jsonSights);


                Deployment.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        //for each sight, place a pin on the map
                        foreach (Sight sight in sights)
                        {
                           
                            var pin = new Pushpin();
                            pin.Location = new GeoCoordinate(sight.Latitude, sight.Longitude);
                            pin.Content = sight.Name;
                            sightsMap.Children.Add(pin);

                            //add a handler for when pin is clicked, select that sight
                            pin.AddHandler(Pushpin.MouseLeftButtonUpEvent, new MouseButtonEventHandler(PinClick), true);
                            
                            NotificationTextBlock.Text = "";

                        }
                    })
                );
                

                //show the map and buttons for map on the page
                Deployment.Current.Dispatcher.BeginInvoke(
                   new Action(() =>
                   {
                       sightsMap.Visibility = Visibility.Visible;
                       btn_ZoomInButton.Visibility = Visibility.Visible;
                       btn_ZoomOutButton.Visibility = Visibility.Visible;
                       btn_ChangeToRoadViewButton.Visibility = Visibility.Visible;
                       btn_ChangeToAerialViewButton.Visibility = Visibility.Visible;

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

        //event handler for left button up on mouse for the pin
        private void PinClick(object sender, MouseButtonEventArgs e)
        {
            Pushpin pin = (Pushpin)sender;

            Sight selection = null;

            //find the appropriate sight from the list of sights with the pin's coordinate
            foreach (Sight sight in sights)
            {
                if (pin.Location.Latitude == sight.Latitude & pin.Location.Longitude == sight.Longitude)
                {
                    selection = sight;
                }
            }

            //select that sight and go back to main page
            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    App.SelectedSight = selection;
                    this.NavigationService.GoBack();

                })
            );
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

        /// <summary>
        /// Zoom in to the map on the center of the map element
        /// </summary>
        private void btn_ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            sightsMap.ZoomLevel = sightsMap.ZoomLevel + 1;
        }

        /// <summary>
        /// Zoom out from the map on the center of the map element
        /// </summary>
        private void btn_ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            sightsMap.ZoomLevel = sightsMap.ZoomLevel - 1;
        }

        /// <summary>
        /// Changes the map view to Road mode
        /// </summary>
        private void btn_ChangeToRoadViewButton_Click(object sender, RoutedEventArgs e)
        {
            sightsMap.Mode = new RoadMode();
        }


        /// <summary>
        /// Changes the map view to Aerial mode
        /// </summary>
        private void btn_ChangeToAerialViewButton_Click(object sender, RoutedEventArgs e)
        {
            sightsMap.Mode = new AerialMode();
        }

    }
}