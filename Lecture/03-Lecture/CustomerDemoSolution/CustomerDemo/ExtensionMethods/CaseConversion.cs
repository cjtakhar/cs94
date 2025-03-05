namespace CustomerDemo.ExtensionMethods
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a camel case string to pascal case.
        /// </summary>
        /// <param name="camelCaseString"></param>
        /// <returns>A pascal case string</returns>
        public static string? ToPascalCase(this string camelCaseString)
        {
            if (camelCaseString?.Length > 0)
            {
                string pascalCaseString = camelCaseString.Substring(0, 1).ToUpperInvariant() + camelCaseString.Substring(1);

                return pascalCaseString;
            }
            return camelCaseString;
        }

        /// <summary>
        /// Converts a pascal case string string to camel case
        /// </summary>
        /// <param name="pascalCaseString"></param>
        /// <returns>A camel case string</returns>
        public static string? ToCamelCase(this string pascalCaseString)
        {
            if (pascalCaseString?.Length > 0)
            {
                string camelCaseString = pascalCaseString.Substring(0, 1).ToLowerInvariant() + pascalCaseString.Substring(1);

                return camelCaseString;
            }
            return pascalCaseString;
        }

        /// <summary>
        /// Removes leading charcters used when an attribute is "not found"
        /// </summary>
        /// <param name="modelStateKey">The model state key</param>
        /// <returns>The model state key with leading characters removed if a result of a not found error</returns>
        public static string? CleanseModelStateKey(this string modelStateKey)
        {
            if (modelStateKey?.StartsWith("$.") == true)
            {
                return modelStateKey.Substring(2);
            }
            return modelStateKey;
        }
    }
}
