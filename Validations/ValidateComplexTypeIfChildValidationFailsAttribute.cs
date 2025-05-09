using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class ValidateComplexTypeIfChildValidationFailsAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null)
        {
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var validationContextForNestedObject = new ValidationContext(value, null, null);

            Validator.TryValidateObject(value, validationContextForNestedObject, validationResults, validateAllProperties: true);

            if (validationResults.Count > 0)
            {
                // At least one child property validation failed, so the complex type is considered valid
                return ValidationResult.Success;
            }
        }

        // No child property validation failed, so the complex type is considered invalid
        return new ValidationResult(ErrorMessage);
    }
}
