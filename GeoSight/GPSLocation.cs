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
        #region Private static member variables

        /// <summary>
        /// A constant used to convert from lat/long units to meters.
        /// </summary>
        private static double p = 3960 * 1609.344;

        #endregion

        #region Private member variables

        /// <summary>
        /// Watches changes in the current GPS location.
        /// Use IGeoPositionWatcher with emulator and GeoCoordinateWatcher with real device.
        /// </summary>
        // GeoCoordinateWatcher watcher;
        private IGeoPositionWatcher<GeoCoordinate> watcher;

        #endregion

        #region Private methods

        /// <summary>
        /// Called when the GPS position changes.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> eventArgs)
        {
            PrintPosition(eventArgs.Position.Location.Latitude, eventArgs.Position.Location.Longitude);

            // Update the current GPS position.
            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    App.CurrentLatitude = eventArgs.Position.Location.Latitude;
                    App.CurrentLongitude = eventArgs.Position.Location.Longitude;
                }));

            // Notify the user if arrived at destination.
            DestinationArrivedNotification(
                App.CurrentLatitude,
                App.CurrentLongitude,
                eventArgs.Position.Location.Latitude,
                eventArgs.Position.Location.Longitude);
        }

        /// <summary>
        /// Notify the user if arrived at destination.
        /// </summary>
        /// <param name="previousLatitude"></param>
        /// <param name="previousLongitude"></param>
        /// <param name="currentLatitude"></param>
        /// <param name="currentLongitude"></param>
        private void DestinationArrivedNotification(
            double previousLatitude,
            double previousLongitude,
            double currentLatitude,
            double currentLongitude)
        {
            // If no sight is selected, there's no destination.
            if (App.SelectedSight == null)
            {
                return;
            }

            double previousDistance = CalculateDistance(
                previousLatitude,
                previousLongitude,
                App.SelectedSight.Latitude,
                App.SelectedSight.Longitude);
            double currentDistance = CalculateDistance(
                currentLatitude,
                currentLongitude,
                App.SelectedSight.Latitude,
                App.SelectedSight.Longitude);
            double radius = App.SelectedSight.Radius;

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
        /// Called when the GPS status changes.
        /// </summary>
        /// <param name="sender">The notifying object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs eventArgs)
        {
            switch (eventArgs.Status)
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
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        private void PrintPosition(double latitude, double longitude)
        {
            Debug.WriteLine("Latitude: {0}, Longitude {1}", latitude, longitude);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GPSLocation()
        {
            // this.watcher = new GeoCoordinateWatcher();
            // Use GpsEmulatorClient's GeocoordinateWatcher in emulator.
            this.watcher = new GpsEmulatorClient.GeoCoordinateWatcher();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            this.watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            this.Started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!this.Started)
            {
                Debug.WriteLine("GeoCoordinateWatcher timed out on start.");
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Calculates the distance between two points on the surface of
        /// the Earth (in meters).
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
            // Current θ
            double currentLongitudeAngle = longitude1;
            // Current φ
            double currentLatitudeAngle = 90 - latitude1;

            // Convert to radians.
            currentLongitudeAngle = currentLongitudeAngle * (2 * Math.PI) / 360;
            currentLatitudeAngle = currentLatitudeAngle * (2 * Math.PI) / 360;

            // Destination θ
            double destinationLongitudeAngle = longitude2;
            // Destination φ
            double destinationLatitudeAngle = 90 - latitude2;

            // Convert to radians.
            destinationLongitudeAngle = destinationLongitudeAngle * (2 * Math.PI)/360;
            destinationLatitudeAngle = destinationLatitudeAngle * (2 * Math.PI)/360;

            // Return the arc length.
            return p * CalculateArcLength(
                currentLatitudeAngle,
                currentLongitudeAngle,
                destinationLatitudeAngle,
                destinationLongitudeAngle);
        }

        /// <summary>
        /// Calculates the distance between two points on the surface of
        /// the Earth (in lat/long units).
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

        #endregion

        #region Public properties

        /// <summary>
        /// True if the watcher was started successfully.
        /// i.e. if it's listening for changes the GPS location.
        /// </summary>
        public bool Started { get; set; }

        #endregion
    }
}
