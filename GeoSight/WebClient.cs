using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace GeoSight
{
    /// <summary>
    /// The RequestState class passes data across async calls.
    /// </summary>
    public class RequestState
    {
        public String Vars { get; set; }
        public HttpWebRequest Request { get; set; }
        public EventDelegates.HTTPResponseDelegate ResponseDelegate { get; set; }
        public EventDelegates.HTTPFailDelegate FailDelegate { get; set; }
    }

    /// <summary>
    /// The WebClient class is used to send HTTP requests asynchronously.
    /// </summary>
    public class WebClient
    {
        #region Private static member variables

        /// <summary>
        /// The default protocol for an HTTP request.
        /// </summary>
        private static string protocol = "http://";

        #endregion

        #region Send request function

        /// <summary>
        /// Sends an HTTP request to the specified URL.
        /// </summary>
        /// <param name="isPOST">True if this is a POST request,
        /// false if it's a GET request.</param>
        /// <param name="url">The URL to which to send the request.</param>
        /// <param name="vars">The POST or GET variables.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void SendReqest(
            bool isPOST,
            String url,
            Dictionary<String, String> vars,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Local variables
            StringBuilder builder;

            // From the given dictionary, create a variable string of
            // the form "name1=var1&name2=var2&..."
            builder = new StringBuilder();
            bool first = true;
            foreach (KeyValuePair<String, String> entry in vars)
            {
                if (first)
                    first = false;
                else
                    builder.Append("&");
                builder.Append(entry.Key);
                builder.Append("=");
                builder.Append(entry.Value);
            }
            String varString = builder.ToString();

            // Create the full correct URI.
            builder = new StringBuilder();
            if (isPOST)
            {
                builder.Append(protocol);
                builder.Append(url);
            }
            else
            {
                builder.Append(protocol);
                builder.Append(url);
                builder.Append("?");
                builder.Append(varString);
            }
            String uri = builder.ToString();

            try
            {
                // Create and initialize an HTTP request to the desired URI.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                if (isPOST)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                else
                {
                    request.Method = "GET";
                }

                // Create a request state.  
                RequestState requestState = new RequestState();
                requestState.Vars = varString;
                requestState.Request = request;
                requestState.ResponseDelegate = responseDelegate;
                requestState.FailDelegate = failDelegate;

                // Start the asynchronous request.
                if (isPOST)
                    request.BeginGetRequestStream(new AsyncCallback(WritePOSTVars), requestState);
                else
                    request.BeginGetResponse(new AsyncCallback(ReadResponse), requestState);
            }
            catch (Exception e)
            {
                failDelegate(e.Message);
            }
        }

        #endregion

        #region Callback functions

        /// <summary>
        /// Writes the POST variables to the body of a POST request.
        /// </summary>
        /// <param name="asynchronousResult">The status of the async operation.</param>
        private void WritePOSTVars(IAsyncResult asynchronousResult)
        {
            RequestState requestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest request = requestState.Request;
                Stream stream = request.EndGetRequestStream(asynchronousResult);
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(requestState.Vars);
                writer.Flush();
                writer.Close();
                request.BeginGetResponse(new AsyncCallback(ReadResponse), requestState);
            }
            catch (Exception e)
            {
                requestState.FailDelegate(e.Message);
            }
        }

        /// <summary>
        /// Reads the HTTP response from the server.
        /// </summary>
        /// <param name="asynchronousResult">The status of the async operation.</param>
        private void ReadResponse(IAsyncResult asynchronousResult)
        {
            RequestState requestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest request = requestState.Request;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                Stream responseStream = response.GetResponseStream();
                requestState.ResponseDelegate(responseStream);
                response.Close();
            }
            catch (Exception e)
            {
                requestState.FailDelegate(e.Message);
            }
        }

        #endregion
    }
}
