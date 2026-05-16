using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Validation;

public static class RequestValidator
{
    public static IResult? Validate(object instance)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        if (Validator.TryValidateObject(instance, context, results, validateAllProperties: true))
        {
            return null;
        }

        var errors = results
            .Where(r => !string.IsNullOrEmpty(r.ErrorMessage))
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                g => g.Select(x => x.ErrorMessage!).Distinct().ToArray());

        return Results.ValidationProblem(errors);
    }
}
