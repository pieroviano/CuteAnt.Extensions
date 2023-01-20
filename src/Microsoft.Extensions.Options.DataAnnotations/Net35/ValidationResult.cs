#if NET35
using System.Collections.Generic;

namespace System.ComponentModel.DataAnnotations
{
    public class ValidationResult
    {
        private IEnumerable<string> _memberNames;
        private string _errorMessage;
        /// <summary>Represents the success of the validation (<see langword="true" /> if validation was successful; otherwise, <see langword="false" />).</summary>
        public static readonly ValidationResult Success;

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using an error message.</summary>
        /// <param name="errorMessage">The error message.</param>
        public ValidationResult(string errorMessage)
            : this(errorMessage, (IEnumerable<string>)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using an error message and a list of members that have validation errors.</summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="memberNames">The list of member names that have validation errors.</param>
        public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
        {
            this._errorMessage = errorMessage;
            this._memberNames = (IEnumerable<string>)((object)memberNames ?? (object)new string[0]);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class by using a <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> object.</summary>
        /// <param name="validationResult">The validation result object.</param>
        protected ValidationResult(ValidationResult validationResult)
        {
            this._errorMessage = validationResult != null ? validationResult._errorMessage : throw new ArgumentNullException(nameof(validationResult));
            this._memberNames = validationResult._memberNames;
        }

        /// <summary>Gets the collection of member names that indicate which fields have validation errors.</summary>
        /// <returns>The collection of member names that indicate which fields have validation errors.</returns>
        public IEnumerable<string> MemberNames
        {
            get => this._memberNames;
        }

        /// <summary>Gets the error message for the validation.</summary>
        /// <returns>The error message for the validation.</returns>
        public string ErrorMessage
        {
            get => this._errorMessage;
            set => this._errorMessage = value;
        }

        /// <summary>Returns a string representation of the current validation result.</summary>
        /// <returns>The current validation result.</returns>
        public override string ToString() => this.ErrorMessage ?? base.ToString();
    }
}
#endif