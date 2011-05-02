using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GeoSight
{
    /// <summary>
    /// A class representing a collection of sights.
    /// </summary>
    public class Sights : ObservableCollection<Sight>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonSights">Information about several sights in JSON
        /// format.</param>
        /// <param name="currentLatitude">The current latitude.</param>
        /// <param name="currentLongitude">the current longitude.</param>
        public Sights(JArray jsonSights, double currentLatitude, double currentLongitude)
        {
            for (int i = 0; i <= jsonSights.Count - 1; i++)
            {
                this.Add(new Sight(jsonSights[i], currentLatitude, currentLongitude));
            }
        }

        /// <summary>
        /// Sorts a collection of sights according to their distance to the
        /// current location.
        /// </summary>
        /// <param name="sights">The collection of sights</param>
        /// <returns>The sorted collection of sights</returns>
        public static ObservableCollection<Sight> GetSortedSights(Sights sights)
        {
            // A key selector that creates a key from the distance between the
            // current location and the location of the given sight
            Func<Sight, double> distanceToCurrentLocation = delegate(Sight sight)
            {
                return sight.Distance;
            };

            // Sort the given sights according to their distance to the
            // current location.
            IOrderedEnumerable<Sight> sortedSights = Enumerable.OrderBy(sights, distanceToCurrentLocation);

            // Construct and return the result as an observable collection.
            ObservableCollection<Sight> result = new ObservableCollection<Sight>();
            foreach (Sight sight in sortedSights)
                result.Add(sight);
            return result;
        }
    }
}
