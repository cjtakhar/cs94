using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorldAzureFunctions2.DataTransferObjects
{

    /// <summary>
    /// The error response definition
    /// </summary>
    public class ErrorResponse
    {
        public int errorNumber { get; set; }
        public string parameterName { get; set; }
        public string parameterValue { get; set; }
        public string errorDescription { get; set; }       

    }
}
