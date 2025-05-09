using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Validations;
[AttributeUsage(AttributeTargets.Property)]
public class RequiredForPostAttribute : RequiredAttribute
{
    private readonly string[] _allowedMethods = { "POST" };

    public RequiredForPostAttribute()
    {
    }

    public override bool IsValid(object value)
    {
        var httpContextAccessor = new HttpContextAccessor();
        var httpMethod = httpContextAccessor.HttpContext?.Request.Method;

        if (_allowedMethods.Contains(httpMethod, StringComparer.OrdinalIgnoreCase))
        {
            return base.IsValid(value);
        }

        return true;
    }
}
