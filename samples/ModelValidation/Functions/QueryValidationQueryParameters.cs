using System.ComponentModel.DataAnnotations;

namespace Samples.ModelValidation.Functions
{
    /// <summary>
    /// An object representing the query parameters for the function.
    /// </summary>
    public class QueryValidationQueryParameters
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <remarks>
        /// This is marked with the <see cref="RequiredAttribute"/> attribute, so it is mandatory.
        /// </remarks>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <remarks>
        /// Since this is not marked with the <see cref="RequiredAttribute"/> attribute, it is optional.
        /// </remarks>
        public string Description { get; set; }
    }
}
