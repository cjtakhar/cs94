using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastRest
{
    /// <summary>
    /// This class is used to customize the functionality of the WeatherForecastRestClient
    /// that is automatically generated
    /// </summary>
    public partial class WeatherForecastRestClient
    {
        private string? _lastLocationHeader = null;
        private int _lastStatusCode = -1;

        /// <summary>
        /// The last location header received from a successful 201 Created response
        /// </summary>
        /// <remarks>This is not thread safe and won't work with concurrent post requests</remarks>
        public string? LastLocationHeader
        {
            get { return _lastLocationHeader; }
        }

        /// <summary>
        /// Returns the last status code received
        /// </summary>
        public int LastStatusCode
        {
            get { return _lastStatusCode; }
        }

        /// <summary>
        /// Retrieve the location header from a successful 201 Created response and save it
        /// </summary>
        /// <param name="client">The http client</param>
        /// <param name="response">The response</param>
        /// <remarks>This approach is not thread safe and won't work with concurrent
        /// post requests</remarks>
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response)
        {
            _lastStatusCode = (int)response.StatusCode;
            if (_lastStatusCode == StatusCodes.Status201Created)
            {
                _lastLocationHeader = response.Headers.Location?.ToString();
            }
        }
    }
}
