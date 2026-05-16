using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

public sealed class ClinicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public class CreateClinicRequest
{
    [Required(ErrorMessage = "Poliklinik adı zorunludur.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public sealed class UpdateClinicRequest : CreateClinicRequest
{
}

public sealed class ClinicListQuery
{
    public string? Search { get; set; }
}
