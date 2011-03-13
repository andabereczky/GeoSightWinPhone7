using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;

namespace GeoSight
{
    /// <summary>
    /// Supplies location data that is based on latitude and longitude.
    /// </summary>
    public class GPSLocation
    {
        /// <summary>
        /// Watches changes in the current GPS location.
        /// </summary>
        GeoCoordinateWatcher watcher;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GPSLocation()
        {
            this.watcher = new GeoCoordinateWatcher();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            this.watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            bool started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
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
            PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
            Deployment.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    App.CurrentLatitude = e.Position.Location.Latitude;
                    App.CurrentLongitude = e.Position.Location.Longitude;
                }));
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
