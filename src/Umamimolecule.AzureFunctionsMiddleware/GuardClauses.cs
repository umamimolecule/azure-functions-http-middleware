using System;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Contains methods to validate parameter values.
    /// </summary>
    internal static class GuardClauses
    {
        /// <summary>
        /// Validates a parameter value is not null.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public static void IsNotNull(string paramName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Validates a parameter value is not null or whitespace.
        /// </summary>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public static void IsNotNullOrWhitespace(string paramName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"The parameter {paramName} must not be null or whitespace.", paramName);
            }
        }
    }
}
