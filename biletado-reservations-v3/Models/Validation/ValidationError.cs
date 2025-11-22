namespace biletado_reservations_v3.Models.Validation;

public record ValidationError(string Code, string Message, string MoreInfo);

public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();

    public static ValidationResult Ok() 
        => new ValidationResult { IsValid = true };

    public static ValidationResult Fail(IEnumerable<ValidationError> errors)
        => new ValidationResult { IsValid = false, Errors = errors.ToList() };
}
