using System;
using System.IO;

namespace GeoSight
{
    /// <summary>
    /// A class containing delegate functions that are used as callbacks
    /// in async operations.
    /// </summary>
    public class EventDelegates
    {
        /// <summary>
        /// A delegate used as a callback when an HTTP response is received.
        /// </summary>
        /// <param name="responseStream">The HTTP response stream.</param>
        public delegate void HTTPResponseDelegate(Stream responseStream);

        /// <summary>
        /// A delegate used as a callback when an HTTP request fails.
        /// </summary>
        /// <param name="message">A message that contains the reason
        /// for the failure.</param>
        public delegate void HTTPFailDelegate(String message);
    }
}
