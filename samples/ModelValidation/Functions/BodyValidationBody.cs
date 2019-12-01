using System.ComponentModel.DataAnnotations;

namespace Samples.ModelValidation.Functions
{
    /// <summary>
    /// An object representing the body payload for the function.
    /// </summary>
    /// <example>
    /// The following JSON structures are valid examples of this payload:
    /// 
    /// {
    ///   "name": "Test"
    /// }
    ///
    /// {
    ///   "name": "Test",
    ///   "user": {
    ///     "id": 1
    ///   }
    /// }
    /// 
    /// {
    ///   "name": "Test",
    ///   "user": {
    ///     "id": 1,
    ///     "name": "Fred"
    ///   }
    /// }
    /// 
    /// </example>
    public class BodyValidationBody
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
        /// Gets or sets the user data.
        /// </summary>
        /// <remarks>
        /// This is marked with the <see cref="RequiredAttribute"/> attribute, so it is mandatory.
        /// However if it is supplied, then an ID must be supplied.
        /// </remarks>
        public UserInfo User { get; set; }

        public class UserInfo
        {
            [Required]
            public int ID { get; set; }

            [Required]
            public string Name { get; set; }

            public string Description { get; set; }
        }
    }    
}
