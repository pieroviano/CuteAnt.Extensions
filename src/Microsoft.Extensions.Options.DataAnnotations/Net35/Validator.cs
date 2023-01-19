#if NET35
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Resources;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>Defines a helper class that can be used to validate objects, properties, and methods when it is included in their associated <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute" /> attributes.</summary>
    public static class Validator
    {
        private static ValidationAttributeStore _store = ValidationAttributeStore.Instance;

        /// <summary>Validates the property.</summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context that describes the property to validate.</param>
        /// <param name="validationResults">A collection to hold each failed validation.</param>
        /// <returns>
        /// <see langword="true" /> if the property validates; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///         <paramref name="value" /> cannot be assigned to the property.
        /// -or-
        /// <paramref name="value" /> is <see langword="null" />.</exception>
        public static bool TryValidateProperty(
          object value,
          ValidationContext validationContext,
          ICollection<ValidationResult> validationResults)
        {
            Type propertyType = Validator._store.GetPropertyType(validationContext);
            Validator.EnsureValidPropertyType(validationContext.MemberName, propertyType, value);
            bool flag = true;
            bool breakOnFirstError = validationResults == null;
            IEnumerable<ValidationAttribute> validationAttributes = Validator._store.GetPropertyValidationAttributes(validationContext);
            foreach (Validator.ValidationError validationError in Validator.GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        /// <summary>Determines whether the specified object is valid using the validation context and validation results collection.</summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationResults">A collection to hold each failed validation.</param>
        /// <returns>
        /// <see langword="true" /> if the object validates; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="instance" /> is <see langword="null" />.</exception>
        public static bool TryValidateObject(
          object instance,
          ValidationContext validationContext,
          ICollection<ValidationResult> validationResults)
        {
            return Validator.TryValidateObject(instance, validationContext, validationResults, false);
        }

        /// <summary>Determines whether the specified object is valid using the validation context, validation results collection, and a value that specifies whether to validate all properties.</summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationResults">A collection to hold each failed validation.</param>
        /// <param name="validateAllProperties">
        /// <see langword="true" /> to validate all properties; if <see langword="false" />, only required attributes are validated.</param>
        /// <returns>
        /// <see langword="true" /> if the object validates; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="instance" /> is <see langword="null" />.</exception>
        public static bool TryValidateObject(
          object instance,
          ValidationContext validationContext,
          ICollection<ValidationResult> validationResults,
          bool validateAllProperties)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (validationContext != null && instance != validationContext.ObjectInstance)
                throw new ArgumentException("DataAnnotationsResources.Validator_InstanceMustMatchValidationContextInstance", nameof(instance));
            bool flag = true;
            bool breakOnFirstError = validationResults == null;
            foreach (Validator.ValidationError objectValidationError in Validator.GetObjectValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(objectValidationError.ValidationResult);
            }
            return flag;
        }

        /// <summary>Returns a value that indicates whether the specified value is valid with the specified attributes.</summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationResults">A collection to hold failed validations.</param>
        /// <param name="validationAttributes">The validation attributes.</param>
        /// <returns>
        /// <see langword="true" /> if the object validates; otherwise, <see langword="false" />.</returns>
        public static bool TryValidateValue(
          object value,
          ValidationContext validationContext,
          ICollection<ValidationResult> validationResults,
          IEnumerable<ValidationAttribute> validationAttributes)
        {
            bool flag = true;
            bool breakOnFirstError = validationResults == null;
            foreach (Validator.ValidationError validationError in Validator.GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        /// <summary>Validates the property.</summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context that describes the property to validate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="value" /> cannot be assigned to the property.</exception>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The <paramref name="value" /> parameter is not valid.</exception>
        public static void ValidateProperty(object value, ValidationContext validationContext)
        {
            Type propertyType = Validator._store.GetPropertyType(validationContext);
            Validator.EnsureValidPropertyType(validationContext.MemberName, propertyType, value);
            IEnumerable<ValidationAttribute> validationAttributes = Validator._store.GetPropertyValidationAttributes(validationContext);
            Validator.GetValidationErrors(value, validationContext, validationAttributes, false).FirstOrDefault<Validator.ValidationError>()?.ThrowValidationException();
        }

        /// <summary>Determines whether the specified object is valid using the validation context.</summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The object is not valid.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="instance" /> is <see langword="null" />.</exception>
        public static void ValidateObject(object instance, ValidationContext validationContext) => Validator.ValidateObject(instance, validationContext, false);

        /// <summary>Determines whether the specified object is valid using the validation context, and a value that specifies whether to validate all properties.</summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validateAllProperties">
        /// <see langword="true" /> to validate all properties; otherwise, <see langword="false" />.</param>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">
        /// <paramref name="instance" /> is not valid.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="instance" /> is <see langword="null" />.</exception>
        public static void ValidateObject(
          object instance,
          ValidationContext validationContext,
          bool validateAllProperties)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            if (instance != validationContext.ObjectInstance)
                throw new ArgumentException("DataAnnotationsResources.Validator_InstanceMustMatchValidationContextInstance", nameof(instance));
            Validator.GetObjectValidationErrors(instance, validationContext, validateAllProperties, false).FirstOrDefault<Validator.ValidationError>()?.ThrowValidationException();
        }

        /// <summary>Validates the specified attributes.</summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationAttributes">The validation attributes.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="validationContext" /> parameter is <see langword="null" />.</exception>
        /// <exception cref="T:System.ComponentModel.DataAnnotations.ValidationException">The <paramref name="value" /> parameter does not validate with the <paramref name="validationAttributes" /> parameter.</exception>
        public static void ValidateValue(
          object value,
          ValidationContext validationContext,
          IEnumerable<ValidationAttribute> validationAttributes)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            Validator.GetValidationErrors(value, validationContext, validationAttributes, false).FirstOrDefault<Validator.ValidationError>()?.ThrowValidationException();
        }

        internal static ValidationContext CreateValidationContext(
          object instance,
          ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            return new ValidationContext(instance, (IServiceProvider)validationContext, validationContext.Items);
        }

        private static bool CanBeAssigned(Type destinationType, object value)
        {
            if (destinationType == (Type)null)
                throw new ArgumentNullException(nameof(destinationType));
            if (value != null)
                return destinationType.IsAssignableFrom(value.GetType());
            if (!destinationType.IsValueType)
                return true;
            return destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static void EnsureValidPropertyType(
          string propertyName,
          Type propertyType,
          object value)
        {
            if (!Validator.CanBeAssigned(propertyType, value))
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.Validator_Property_Value_Wrong_Type", new object[2]
                {
          (object) propertyName,
          (object) propertyType
                }), nameof(value));
        }

        private static IEnumerable<Validator.ValidationError> GetObjectValidationErrors(
          object instance,
          ValidationContext validationContext,
          bool validateAllProperties,
          bool breakOnFirstError)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            List<Validator.ValidationError> source = new List<Validator.ValidationError>();
            source.AddRange(Validator.GetObjectPropertyValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError));
            if (source.Any<Validator.ValidationError>())
                return (IEnumerable<Validator.ValidationError>)source;
            IEnumerable<ValidationAttribute> validationAttributes = Validator._store.GetTypeValidationAttributes(validationContext);
            source.AddRange(Validator.GetValidationErrors(instance, validationContext, validationAttributes, breakOnFirstError));
            if (source.Any<Validator.ValidationError>())
                return (IEnumerable<Validator.ValidationError>)source;
            if (instance is IValidatableObject validatableObject)
            {
                foreach (ValidationResult validationResult in validatableObject.Validate(validationContext).Where<ValidationResult>((Func<ValidationResult, bool>)(r => r != ValidationResult.Success)))
                    source.Add(new Validator.ValidationError((ValidationAttribute)null, instance, validationResult));
            }
            return (IEnumerable<Validator.ValidationError>)source;
        }

        private static IEnumerable<Validator.ValidationError> GetObjectPropertyValidationErrors(
          object instance,
          ValidationContext validationContext,
          bool validateAllProperties,
          bool breakOnFirstError)
        {
            ICollection<KeyValuePair<ValidationContext, object>> propertyValues = Validator.GetPropertyValues(instance, validationContext);
            List<Validator.ValidationError> source = new List<Validator.ValidationError>();
            foreach (KeyValuePair<ValidationContext, object> keyValuePair in (IEnumerable<KeyValuePair<ValidationContext, object>>)propertyValues)
            {
                IEnumerable<ValidationAttribute> validationAttributes = Validator._store.GetPropertyValidationAttributes(keyValuePair.Key);
                if (validateAllProperties)
                    source.AddRange(Validator.GetValidationErrors(keyValuePair.Value, keyValuePair.Key, validationAttributes, breakOnFirstError));
                else if (validationAttributes.FirstOrDefault<ValidationAttribute>((Func<ValidationAttribute, bool>)(a => a is RequiredAttribute)) is RequiredAttribute requiredAttribute)
                {
                    ValidationResult validationResult = GetValidationResult(requiredAttribute, keyValuePair.Value, keyValuePair.Key);
                    if (validationResult != ValidationResult.Success)
                        source.Add(new Validator.ValidationError((ValidationAttribute)requiredAttribute, keyValuePair.Value, validationResult));
                }
                if (breakOnFirstError)
                {
                    if (source.Any<Validator.ValidationError>())
                        break;
                }
            }
            return (IEnumerable<Validator.ValidationError>)source;
        }

        public static ValidationResult GetValidationResult(RequiredAttribute attribute,
            object value,
            ValidationContext validationContext)
        {
            ValidationResult validationResult = validationContext != null ? IsValid(attribute, value, validationContext) : throw new ArgumentNullException(nameof(validationContext));
            if (validationResult != null && (validationResult == null || string.IsNullOrEmpty(validationResult.ErrorMessage)))
                validationResult = new ValidationResult(attribute.FormatErrorMessage(validationContext.DisplayName), validationResult.MemberNames);
            return validationResult;
        }

        public static ValidationResult GetValidationResult(ValidationAttribute attribute,
            object value,
            ValidationContext validationContext)
        {
            ValidationResult validationResult = validationContext != null ? IsValid(attribute, value, validationContext) : throw new ArgumentNullException(nameof(validationContext));
            if (validationResult != null && (validationResult == null || string.IsNullOrEmpty(validationResult.ErrorMessage)))
                validationResult = new ValidationResult(attribute.FormatErrorMessage(validationContext.DisplayName), validationResult.MemberNames);
            return validationResult;
        }

        private static ValidationResult IsValid(RequiredAttribute attribute,
            object value,
            ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            if (!attribute.IsValid(value))
            {
                string[] strArray;
                if (validationContext.MemberName == null)
                    strArray = (string[])null;
                else
                    strArray = new string[1]
                    {
                        validationContext.MemberName
                    };
                string[] memberNames = strArray;
                validationResult = new ValidationResult(attribute.FormatErrorMessage(validationContext.DisplayName), (IEnumerable<string>)memberNames);
            }
            return validationResult;
        }

        private static ValidationResult IsValid(ValidationAttribute attribute,
            object value,
            ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            if (!attribute.IsValid(value))
            {
                string[] strArray;
                if (validationContext.MemberName == null)
                    strArray = (string[])null;
                else
                    strArray = new string[1]
                    {
                        validationContext.MemberName
                    };
                string[] memberNames = strArray;
                validationResult = new ValidationResult(attribute.FormatErrorMessage(validationContext.DisplayName), (IEnumerable<string>)memberNames);
            }
            return validationResult;
        }

        private static ICollection<KeyValuePair<ValidationContext, object>> GetPropertyValues(
          object instance,
          ValidationContext validationContext)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
            List<KeyValuePair<ValidationContext, object>> propertyValues = new List<KeyValuePair<ValidationContext, object>>(properties.Count);
            foreach (PropertyDescriptor propertyDescriptor in properties)
            {
                ValidationContext validationContext1 = Validator.CreateValidationContext(instance, validationContext);
                validationContext1.MemberName = propertyDescriptor.Name;
                if (Validator._store.GetPropertyValidationAttributes(validationContext1).Any<ValidationAttribute>())
                    propertyValues.Add(new KeyValuePair<ValidationContext, object>(validationContext1, propertyDescriptor.GetValue(instance)));
            }
            return (ICollection<KeyValuePair<ValidationContext, object>>)propertyValues;
        }

        private static IEnumerable<Validator.ValidationError> GetValidationErrors(
          object value,
          ValidationContext validationContext,
          IEnumerable<ValidationAttribute> attributes,
          bool breakOnFirstError)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            List<Validator.ValidationError> validationErrors = new List<Validator.ValidationError>();
            Validator.ValidationError validationError;
            var attr = attributes.FirstOrDefault<ValidationAttribute>((Func<ValidationAttribute, bool>)(a => a is RequiredAttribute));
            if (attributes.FirstOrDefault<ValidationAttribute>((Func<ValidationAttribute, bool>)(a => a is RequiredAttribute)) is RequiredAttribute attribute1 && !Validator.TryValidate(value, validationContext, (ValidationAttribute)attribute1, out validationError))
            {

                validationErrors.Add(validationError);
                return (IEnumerable<Validator.ValidationError>)validationErrors;
            }
            else
            foreach (ValidationAttribute attribute2 in attributes)
            {
                if (attribute2 != ((RequiredAttribute)attr) && !Validator.TryValidate(value, validationContext, attribute2, out validationError))
                {
                    validationErrors.Add(validationError);
                    if (breakOnFirstError)
                        break;
                }
            }
            return (IEnumerable<Validator.ValidationError>)validationErrors;
        }

        private static bool TryValidate(
          object value,
          ValidationContext validationContext,
          ValidationAttribute attribute,
          out Validator.ValidationError validationError)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
            ValidationResult validationResult = GetValidationResult(attribute, value, validationContext);
            if (validationResult != ValidationResult.Success)
            {
                validationError = new Validator.ValidationError(attribute, value, validationResult);
                return false;
            }
            validationError = (Validator.ValidationError)null;
            return true;
        }

        private class ValidationError
        {
            internal ValidationError(
              ValidationAttribute validationAttribute,
              object value,
              ValidationResult validationResult)
            {
                this.ValidationAttribute = validationAttribute;
                this.ValidationResult = validationResult;
                this.Value = value;
            }

            internal object Value { get; set; }

            internal ValidationAttribute ValidationAttribute { get; set; }

            internal ValidationResult ValidationResult { get; set; }

            internal void ThrowValidationException() => throw new ValidationException(this.ValidationResult.ErrorMessage, this.ValidationAttribute, this.Value);
        }
    }
}
#endif