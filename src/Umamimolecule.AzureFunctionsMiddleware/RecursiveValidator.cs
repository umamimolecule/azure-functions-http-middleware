using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Umamimolecule.AzureFunctionsMiddleware
{
    /// <summary>
    /// Recursively validates all properties and child properties of a model.
    /// </summary>
    public static class RecursiveValidator
    {
        /// <summary>
        /// Recursively validates <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="results">A collection to hold each failed validation.</param>
        /// <param name="validateAllProperties">true to validate all properties; if false, only required attributes are validated.</param>
        /// <returns>true if the object validates; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static bool TryValidateObject(object instance, ICollection<ValidationResult> results, bool validateAllProperties)
        {
            return TryValidateObject(instance, results, validateAllProperties, null);
        }

        /// <summary>
        /// Recursively validates <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="results">A collection to hold each failed validation.</param>
        /// <param name="validateAllProperties">true to validate all properties; if false, only required attributes are validated.</param>
        /// <param name="prefix">The prefix to append to the field name when validation fails.</param>
        /// <returns>true if the object validates; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        private static bool TryValidateObject(object instance, ICollection<ValidationResult> results, bool validateAllProperties, string prefix)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var tempResults = new List<ValidationResult>();

            ValidationContext validationContext = new ValidationContext(instance);
            var isValid = Validator.TryValidateObject(instance, validationContext, tempResults, validateAllProperties: validateAllProperties);

            foreach (var item in tempResults)
            {
                IEnumerable<string> memberNames = item.MemberNames.Select(name => (!string.IsNullOrEmpty(prefix) ? prefix + "." : string.Empty) + name);
                results.Add(new ValidationResult(item.ErrorMessage, memberNames));
            }

            foreach (var prop in instance.GetType().GetProperties())
            {
                if (prop.PropertyType != typeof(string))
                {
                    var value = prop.GetValue(instance);
                    if (value == null)
                    {
                        continue;
                    }
                    else if (value is IEnumerable<object> list)
                    {
                        var memberPrefix = (!string.IsNullOrEmpty(prefix) ? prefix + "." : string.Empty) + prop.Name;
                        int i = 0;
                        foreach (var item in list)
                        {
                            if (!TryValidateObject(item, results, validateAllProperties, $"{memberPrefix}[{i}]"))
                            {
                                isValid = false;
                            }

                            i++;
                        }
                    }
                    else
                    {
                        var memberPrefix = (!string.IsNullOrEmpty(prefix) ? prefix + "." : string.Empty) + prop.Name;
                        if (!TryValidateObject(value, results, validateAllProperties, memberPrefix))
                        {
                            isValid = false;
                        }
                    }
                }
            }

            return isValid;
        }
    }
}
