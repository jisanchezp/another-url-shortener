using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Common;

public static class ValidationHelper
{
    public static bool TryValidate<T>(T obj, out List<string> errors)
    {
        var context = new ValidationContext(obj!);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(obj!, context, results, validateAllProperties: true);
        errors = [.. results.Select(r => r.ErrorMessage ?? "Invalid value")];
        return isValid;
    }
}