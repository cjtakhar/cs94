using static CustomerDemo.DataTransferObjects.ErrorResponse;

namespace CustomerDemo.DataTransferObjects
{

    public enum ErrorCode
    {
        Undefined = 0,
        StringIsToLong = 1,
        InvalidEmailAddress = 2,
        InvalidRange = 3,
        MustBeProvided = 4,
        MissingName = 5,
        EmailMustContainFirstOrLastName = 6,
        AllInputIsNull = 7
    }
    /// <summary>
    /// The error response definition
    /// </summary>
    public class ErrorResponse
    {
        public ErrorCode errorNumber { get; set; }
        public string parameterName { get; set; } = String.Empty;
        public string parameterValue { get; set; } = String.Empty;
        public string errorDescription { get; set; } = String.Empty;

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static ErrorCode GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (Enum.TryParse<ErrorCode>(encodedErrorDescription, out ErrorCode errorNumber))
            {
                return errorNumber;
            }
            return ErrorCode.Undefined;
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, ErrorCode decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {
            ErrorCode errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);

            switch (errorNumber)
            {
                case ErrorCode.StringIsToLong:
                    {
                        return ("String is to long", errorNumber);
                    }
                case ErrorCode.InvalidEmailAddress:
                    {
                        return ("Invalid email address", errorNumber);
                    }
                case ErrorCode.InvalidRange:
                    {
                        return ("Invalid range", errorNumber);
                    }
                case ErrorCode.MustBeProvided:
                    {
                        return ("Must be provided", errorNumber);
                    }
                case ErrorCode.MissingName:
                    {
                        return ("Missing name", errorNumber);
                    }
                case ErrorCode.EmailMustContainFirstOrLastName:
                    {
                        return ("Email must contain first or last name", errorNumber);
                    }
                case ErrorCode.AllInputIsNull:
                    {
                        return ("All input is null", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }
            }
        }

    }
}
