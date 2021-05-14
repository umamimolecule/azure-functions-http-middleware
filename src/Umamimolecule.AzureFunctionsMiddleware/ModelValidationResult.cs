namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Represents the validation result for a model.
    /// </summary>
    public class ModelValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message when the validation was unsuccessful.
        /// </summary>
        public string Error { get; set; }
    }
}
