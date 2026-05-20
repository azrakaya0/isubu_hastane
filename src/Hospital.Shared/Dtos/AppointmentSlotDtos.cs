namespace Hospital.Shared.Dtos;

public sealed class AppointmentSlotDto
{
    public DateTime ScheduledAt { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
