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
        /// The protocol for an HTTP request. Can be "http" or "https".
        /// </summary>
        private String protocol;

        /// <summary>
        /// The boundary to be used in a multi-part POST request.
        /// </summary>
        private String boundary;

        #endregion

        #region Constructor

        public WebClient(String protocol)
        {
            this.protocol = protocol;
            this.boundary = "------WyDcB6n100wunb48cOn5OeR82ysYaCrm"; // doesn't have to be randomly generated
        }

        #endregion

        #region Public static functions

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A byte array.</returns>
        public static byte[] StrToByteArray(string str)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A string.</returns>
        public static string ByteArrayToStr(byte[] bytes)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(bytes, 0, bytes.Length);
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
        public void SendRequest(
            bool isPOST,
            String url,
            Dictionary<String, String> vars,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Get the variable string.
            String varString = ConstructVarString(vars);

            // Create a binary byte array for the body.
            byte[] body = StrToByteArray(varString);

            // Create the full correct URI.
            String uri = ConstructURI(url, isPOST, varString);

            // Determine the method and content type.
            String method, contentType;
            if (isPOST)
            {
                method = "POST";
                contentType = "application/x-www-form-urlencoded";
            }
            else
            {
                method = "GET";
                contentType = String.Empty;
            }

            // Start the asynchronous call.
            StartAsyncCall(
                uri,
                method,
                contentType,
                body,
                responseDelegate,
                failDelegate);
        }

        /// <summary>
        /// Uploads a file using a POST request to the specified url.
        /// </summary>
        /// <param name="url">The URL to which to send the request.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="varName">The variable name for the file to upload.</param>
        /// <param name="fileContents">The contents of the file.</param>
        /// <param name="vars">Any other POST variables.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        public void UploadFile(
            string url,
            string fileName,
            string varName,
            byte[] fileContents,
            Dictionary<string, string> vars,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            // Create the body of the multi-part POST request.
            byte[] body = ConstructMultiPartBody(fileName, varName, fileContents, vars);

            // Create the full correct URI.
            String uri = ConstructURI(url, true, String.Empty);

            // Start the asynchronous call.
            StartAsyncCall(
                uri,
                "POST",
                "multipart/form-data;boundary=" + boundary,
                body,
                responseDelegate,
                failDelegate);
        }

        #endregion

        #region Private member functions

        /// <summary>
        /// From the given dictionary, create a variable string of the form
        /// "name1=var1&name2=var2&..."
        /// </summary>
        /// <param name="vars">The POST or GET variables.</param>
        /// <returns>A variable string that can be used as part of a URL or
        /// the body of an HTTP request.</returns>
        private String ConstructVarString(Dictionary<String, String> vars)
        {
            // Local variables.
            StringBuilder builder = new StringBuilder();
            bool first = true;

            // Build the variable string.
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

            // Escape the variable string.
            String varString = Uri.EscapeUriString(builder.ToString());
            varString = varString.Replace("@", "%40");

            return varString;
        }

        /// <summary>
        /// Create a full correct URI given a URL and a string of GET/POST variables.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="isPOST">True if this is a POST request.</param>
        /// <param name="varString">The string of GET/POST variables.</param>
        /// <returns>The full correct URI.</returns>
        private String ConstructURI(string url, bool isPOST, string varString)
        {
            // Local variables.
            StringBuilder builder = new StringBuilder();

            // Build the URI.
            builder.Append(protocol);
            builder.Append("://");
            builder.Append(url);
            if (!isPOST && varString != String.Empty)
            {
                builder.Append("?");
                builder.Append(varString);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Create the body of the multi-part POST request.
        /// </summary>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="varName">The variable name for the file to upload.</param>
        /// <param name="fileContents">The contents of the file.</param>
        /// <param name="vars">Any other POST variables.</param>
        /// <returns>The body of the multi-part POST request.</returns>
        private byte[] ConstructMultiPartBody(
            string fileName,
            string varName,
            byte[] fileContents,
            Dictionary<string, string> vars)
        {
            // Local variables.
            StringBuilder builder = new StringBuilder();
            String CRLF = "\r\n";
            String twoDashes = "--";

            // Append the other POST variables.
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

            // Append the file.
            builder.Append("Content-Disposition: form-data; name=\"");
            builder.Append(varName);
            builder.Append("\"; filename=\"");
            builder.Append(fileName);
            builder.Append("\"");
            builder.Append(CRLF);
            builder.Append("Content-Type: image/jpeg");
            builder.Append(CRLF);
            builder.Append(CRLF);

            // Build the full body.
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

            return body;
        }

        /// <summary>
        /// Begins an asynchronous call to the specified URI.
        /// </summary>
        /// <param name="uri">The URI to which to send the request.</param>
        /// <param name="method">The HTTP method. Can be "GET" or "POST".</param>
        /// <param name="contentType">The HTTP content type.</param>
        /// <param name="body">The body of a POST request.</param>
        /// <param name="responseDelegate">The delegate that should be
        /// called if the client receives a response.</param>
        /// <param name="failDelegate">The delegate that should be
        /// called if the client fails to receive a response.</param>
        private void StartAsyncCall(
            String uri,
            String method,
            String contentType,
            byte[] body,
            EventDelegates.HTTPResponseDelegate responseDelegate,
            EventDelegates.HTTPFailDelegate failDelegate)
        {
            try
            {
                // Create and initialize an HTTP request to the desired URI.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                request.CookieContainer = App.Cookies;

                // Write some HTTP headers.
                request.Method = method;
                request.ContentType = contentType;

                // Create a request state.  
                RequestState requestState = new RequestState();
                requestState.Body = body;
                requestState.Request = request;
                requestState.ResponseDelegate = responseDelegate;
                requestState.FailDelegate = failDelegate;

                // Start the asynchronous request.
                if (method == "POST")
                    request.BeginGetRequestStream(new AsyncCallback(WriteBody), requestState);
                else if (method == "GET")
                    request.BeginGetResponse(new AsyncCallback(ReadResponse), requestState);
            }
            catch (Exception e)
            {
                failDelegate(e.Message);
            }
        }

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
