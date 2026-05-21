namespace Hospital.Client.Models;

public sealed class SlotViewModel
{
    public DateTime ScheduledAt { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }

    public string DisplayLabel => IsAvailable ? Label : $"{Label} (Dolu)";
    public Color BackgroundColor => IsAvailable ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#F5F5F5");
    public Color TextColor => IsAvailable ? Color.FromArgb("#000000") : Color.FromArgb("#999999");
    public double Opacity => IsAvailable ? 1.0 : 0.6;
}
