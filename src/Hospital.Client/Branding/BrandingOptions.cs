using System.Text.Json.Serialization;

namespace Hospital.Client.Branding;

/// <summary>
/// resources/Raw/branding.json ile Resources/Images altındaki görselleri eşlemek için.
/// Dosya adlarında dizin ayırıcı kullanmayın; yalnızca güvenli dosya adları (ör. hastane.jpeg).
/// </summary>
public sealed class BrandingOptions
{
    [JsonPropertyName("portalHeroImage")]
    public string? PortalHeroImage { get; set; }

    [JsonPropertyName("loginLogoImage")]
    public string? LoginLogoImage { get; set; }

    [JsonPropertyName("portalStaffCardImage")]
    public string? PortalStaffCardImage { get; set; }

    [JsonPropertyName("portalDoctorCardImage")]
    public string? PortalDoctorCardImage { get; set; }

    [JsonPropertyName("portalPatientCardImage")]
    public string? PortalPatientCardImage { get; set; }

    [JsonPropertyName("patientHomeBannerImage")]
    public string? PatientHomeBannerImage { get; set; }

    [JsonPropertyName("doctorHomeBannerImage")]
    public string? DoctorHomeBannerImage { get; set; }

    [JsonPropertyName("adminHomeBannerImage")]
    public string? AdminHomeBannerImage { get; set; }
}
