#if NET35
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
    public interface IValidatableObject
    {
        /// <summary>Determines whether the specified object is valid.</summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext);
    }
}
#endif