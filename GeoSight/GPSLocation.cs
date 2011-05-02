using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using GpsEmulatorClient;

namespace GeoSight
{
    /// <summary>
    /// Supplies location data that is based on latitude and longitude.
    /// </summary>
    public class GPSLocation
    {
        public static double p = 3960 * 1609.344;

        /// <summary>
        /// Watches changes in the current GPS location.
        /// </summary>
        /// Use IGeoPositionWatcher with emulator and GeoCoordinateWatcher with real device
        //GeoCoordinateWatcher watcher;
        IGeoPositionWatcher<GeoCoordinate> watcher;

        /// <summary>
        /// True if the watcher was started successfully
        /// i.e. it's listening for changes the GPS location.
        /// </summary>
        public bool Started { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GPSLocation()
        {
            //use GpsEmulatorClient's GeocoordinateWatcher in emulator
            this.watcher = new GpsEmulatorClient.GeoCoordinateWatcher();
            //this.watcher = new GeoCoordinateWatcher();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            this.watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            this.Started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!this.Started)
            {
                Debug.WriteLine("GeoCoordinateWatcher timed out on start.");
            }
        }

        /// <summary>
        /// Called when the GPS position changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            double PreviousLatitude = App.CurrentLatitude;
            double PreviousLongitude = App.CurrentLongitude;
            PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);

            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    App.CurrentLatitude = e.Position.Location.Latitude;
                    App.CurrentLongitude = e.Position.Location.Longitude;
                }));

            DestinationArrivedNotification(PreviousLatitude, PreviousLongitude, e.Position.Location.Latitude, e.Position.Location.Longitude);
        }


        /// <summary>
        /// Called when the GPS position changes.
        /// if the current location is in the destination range, show message box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DestinationArrivedNotification(double PreviousLatitude, double PreviousLongitude, double CurrentLatitude, double CurrentLongitude)
        {

            double previousDistance = 0;
            double currentDistance = 0;
            double radius = 0;

            if (App.SelectedSight != null)
            {
                previousDistance = CalculateDistance(
                    PreviousLatitude, PreviousLongitude,
                    App.SelectedSight.Latitude, App.SelectedSight.Longitude);
                currentDistance = CalculateDistance(
                    CurrentLatitude, CurrentLongitude,
                    App.SelectedSight.Latitude, App.SelectedSight.Longitude);
                radius = App.SelectedSight.Radius;
                Debug.WriteLine("prev:" + previousDistance);
                Debug.WriteLine("curr:" + currentDistance);

            }

            // If the current distance is in the range...
            if (currentDistance <= radius)
            {
                // If the previous distance is not in the range...
                if (previousDistance > radius)
                {
                    // Alert the user that he/she has arrived at the destination.
                    MessageBox.Show("Arrived at " + App.SelectedSight.Name + "!");
                }
                App.InDestination = true;
            }
            else
            {
                App.InDestination = false;
            }
            
        }

        /// <summary>
        /// Calculates the distance between two points on the surface of
        /// the Earth.
        /// </summary>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude2"></param>
        /// <param name="longitude2"></param>
        /// <returns></returns>
        public static double CalculateDistance(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            //θ
            double currentLongitudeAngle = longitude1;
            //φ
            double currentLatitudeAngle = 90 - latitude1;

            //convert to radians
            currentLongitudeAngle = currentLongitudeAngle * (2 * Math.PI) / 360;
            currentLatitudeAngle = currentLatitudeAngle * (2 * Math.PI) / 360;

            //θ
            double destinationLongitudeAngle = longitude2;
            //φ
            double destinationLatitudeAngle = 90 - latitude2;

            //convert to radians
            destinationLongitudeAngle = destinationLongitudeAngle * (2 * Math.PI)/360;
            destinationLatitudeAngle = destinationLatitudeAngle * (2 * Math.PI)/360;

            return p * CalculateArcLength(
                currentLatitudeAngle,
                currentLongitudeAngle,
                destinationLatitudeAngle,
                destinationLongitudeAngle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitudeAngle1"></param>
        /// <param name="longitudeAngle1"></param>
        /// <param name="latitudeAngle2"></param>
        /// <param name="longitudeAngle2"></param>
        /// <returns></returns>
        public static double CalculateArcLength(
            double latitudeAngle1,
            double longitudeAngle1,
            double latitudeAngle2,
            double longitudeAngle2)
        {
            return Math.Acos(
                Math.Sin(longitudeAngle1) * Math.Sin(longitudeAngle2) * Math.Cos(latitudeAngle1 - latitudeAngle2) +
                Math.Cos(longitudeAngle1) * Math.Cos(longitudeAngle2));
        }

        /// <summary>
        /// Called when the GPS status changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Initializing:
                    Console.WriteLine("GPS status change: Working on location fix.");
                    break;

                case GeoPositionStatus.Ready:
                    Console.WriteLine("GPS status change: Have location.");
                    break;

                case GeoPositionStatus.NoData:
                    Console.WriteLine("GPS status change: No data.");
                    break;

                case GeoPositionStatus.Disabled:
                    Console.WriteLine("GPS status change: Disabled.");
                    break;
            }
        }

        /// <summary>
        /// Helper function, prints the given latitude and longitude.
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        private void PrintPosition(double Latitude, double Longitude)
        {
            Debug.WriteLine("Latitude: {0}, Longitude {1}", Latitude, Longitude);
        }
    }
}
