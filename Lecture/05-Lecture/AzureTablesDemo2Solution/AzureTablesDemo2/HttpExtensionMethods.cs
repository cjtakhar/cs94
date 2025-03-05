using System.Net;

namespace AzureTables2
{
    /// <summary>
    /// Defines some extention methods to assist with Http Calls.
    /// </summary>
    public static class HttpExtensionMethods
    {
        /// <summary>
        /// Determines whether the status code provided indicates success
        /// </summary>
        /// <param name="statusCode">The status code to check</param>
        /// <returns><c>true</c> if statusCode is between greater than or equal to 200 and less than or equal to 299 otherwise, <c>false</c>.</returns>
        public static bool IsSuccessStatusCode(this int statusCode)
        {
            if (statusCode >=200 &&  statusCode <=299)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the status code provided indicates success
        /// </summary>
        /// <param name="statusCode">The status code to check</param>
        /// <returns><c>true</c> if statusCode is between greater than or equal to 200 and less than or equal to 299 otherwise, <c>false</c>.</returns>
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            if ((int)statusCode >= 200 && (int)statusCode <= 299)
            {
                return true;
            }
            return false;
        }

    }
}
