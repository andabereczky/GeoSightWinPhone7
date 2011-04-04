using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace GeoSight
{
    /// <summary>
    /// The RequestState class passes data across async calls.
    /// </summary>
    public class RequestState
    {
        public byte[] Body { get; set; }
        public HttpWebRequest Request { get; set; }
        public EventDelegates.HTTPResponseDelegate ResponseDelegate { get; set; }
        public EventDelegates.HTTPFailDelegate FailDelegate { get; set; }
    }

    /// <summary>
    /// The WebClient class is used to send HTTP requests asynchronously.
    /// </summary>
    public class WebClient
    {
        #region Private member variables

        /// <summary>
        /// The protocol for an HTTP request.
        /// </summary>
        private String protocol;

        #endregion

        #region Constructor

        public WebClient(String protocol)
        {
            this.protocol = protocol;
        }

        #endregion

        #region Public static functions

        public static byte[] StrToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static string ByteArrayToStr(byte[] bytes)
        {
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            return enc.GetString(bytes, 0, bytes.Length);
        }

        #endregion

        #region Public members functions

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
            String varString = Uri.EscapeUriString(builder.ToString());
            varString = varString.Replace("@", "%40"); // HACK

            // Create the full correct URI.
            builder = new StringBuilder();
            if (isPOST)
            {
                builder.Append(protocol);
                builder.Append("://");
                builder.Append(url);
            }
            else
            {
                builder.Append(protocol);
                builder.Append("://");
                builder.Append(url);
                if (varString != String.Empty)
                {
                    builder.Append("?");
                    builder.Append(varString);
                }
            }
            String uri = builder.ToString();

            try
            {
                // Create and initialize an HTTP request to the desired URI.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                request.CookieContainer = App.Cookies;
                if (isPOST)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                else
                {
                    request.Method = "GET";
                }

                // Create a binary byte array for the body.
                byte[] body = StrToByteArray(varString);

                // Create a request state.  
                RequestState requestState = new RequestState();
                requestState.Body = body;
                requestState.Request = request;
                requestState.ResponseDelegate = responseDelegate;
                requestState.FailDelegate = failDelegate;

                // Start the asynchronous request.
                if (isPOST)
                    request.BeginGetRequestStream(new AsyncCallback(WriteBody), requestState);
                else
                    request.BeginGetResponse(new AsyncCallback(ReadResponse), requestState);
            }
            catch (Exception e)
            {
                failDelegate(e.Message);
            }
        }

        /// <summary>
        /// Uploads a file using a POST request to the specified url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <param name="varName"></param>
        /// <param name="fileContents"></param>
        /// <param name="vars"></param>
        /// <param name="responseDelegate"></param>
        /// <param name="failDelegate"></param>
        public void UploadFile(
            string url,
            string fileName,
            string varName,
            byte[] fileContents,
            Dictionary<string, string> vars,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Local variables
            StringBuilder builder;
            String boundary = "------WyDcB6n100wunb48cOn5OeR82ysYaCrm";
            String CRLF = "\r\n";
            String twoDashes = "--";

            // Create the body of the multi-part POST request.
            builder = new StringBuilder();
            builder.Append(twoDashes);
            builder.Append(boundary);
            builder.Append(CRLF);
            foreach (KeyValuePair<string, string> entry in vars)
            {
                builder.Append("Content-Disposition: form-data; name=\"");
                builder.Append(entry.Key);
                builder.Append("\"");
                builder.Append(CRLF);
                builder.Append(CRLF);
                builder.Append(entry.Value);
                builder.Append(CRLF);
                builder.Append(twoDashes);
                builder.Append(boundary);
                builder.Append(CRLF);
            }
            builder.Append("Content-Disposition: form-data; name=\"");
            builder.Append(varName);
            builder.Append("\"; filename=\"");
            builder.Append(fileName);
            builder.Append("\"");
            builder.Append(CRLF);
            builder.Append("Content-Type: image/jpeg");
            builder.Append(CRLF);
            builder.Append(CRLF);
            byte[] body = new byte[builder.Length + fileContents.Length + CRLF.Length + twoDashes.Length + boundary.Length + twoDashes.Length + CRLF.Length];
            Array.Copy(
                StrToByteArray(builder.ToString()),
                body,
                builder.Length);
            Array.Copy(
                fileContents,
                0,
                body,
                builder.Length,
                fileContents.Length);
            Array.Copy(
                StrToByteArray(CRLF),
                0,
                body,
                builder.Length + fileContents.Length,
                CRLF.Length);
            Array.Copy(
                StrToByteArray(twoDashes),
                0,
                body,
                builder.Length + fileContents.Length + CRLF.Length,
                twoDashes.Length);
            Array.Copy(
                StrToByteArray(boundary),
                0,
                body,
                builder.Length + fileContents.Length + CRLF.Length + twoDashes.Length,
                boundary.Length);
            Array.Copy(
                StrToByteArray(twoDashes),
                0,
                body,
                builder.Length + fileContents.Length + CRLF.Length + twoDashes.Length + boundary.Length,
                twoDashes.Length);
            Array.Copy(
                StrToByteArray(CRLF),
                0,
                body,
                builder.Length + fileContents.Length + CRLF.Length + twoDashes.Length + boundary.Length + twoDashes.Length,
                CRLF.Length);

            // Create the full correct URI.
            builder = new StringBuilder();
            builder.Append(protocol);
            builder.Append("://");
            builder.Append(url);
            String uri = builder.ToString();

            try
            {
                // Create and initialize an HTTP request to the desired URI.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                request.CookieContainer = App.Cookies;
                request.Method = "POST";
                request.ContentType = "multipart/form-data;boundary=" + boundary;

                // Create a request state.  
                RequestState requestState = new RequestState();
                requestState.Body = body;
                requestState.Request = request;
                requestState.ResponseDelegate = responseDelegate;
                requestState.FailDelegate = failDelegate;

                // Start the asynchronous request.
                request.BeginGetRequestStream(new AsyncCallback(WriteBody), requestState);
            }
            catch (Exception e)
            {
                failDelegate(e.Message);
            }
        }

        #endregion

        #region Callback functions

        /// <summary>
        /// Writes the body of a POST request.
        /// </summary>
        /// <param name="asynchronousResult">The status of the async operation.</param>
        private void WriteBody(IAsyncResult asynchronousResult)
        {
            RequestState requestState = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest request = requestState.Request;
                Stream stream = request.EndGetRequestStream(asynchronousResult);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(requestState.Body);
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
