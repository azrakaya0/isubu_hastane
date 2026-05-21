using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Shared;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

/// <summary>
/// Randevu sayfalarında tarih ve 15 dk slot seçimini yönetir.
/// </summary>
internal sealed class AppointmentSlotPickerHelper
{
    private readonly DatePicker _datePicker;
    private readonly Label? _slotHintLabel;
    private readonly Func<Task<int?>> _getDoctorIdAsync;
    private readonly Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> _fetchSlotsAsync;
    public readonly ObservableCollection<SlotViewModel> Slots = new();
    
    private int? _excludeAppointmentId;
    private SlotViewModel? _selectedSlot;

    // New constructor for CollectionView-based pages
    public AppointmentSlotPickerHelper(
        DatePicker datePicker,
        Func<Task<int?>> getDoctorIdAsync,
        Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> fetchSlotsAsync,
        Label? slotHintLabel = null)
    {
        _datePicker = datePicker;
        _getDoctorIdAsync = getDoctorIdAsync;
        _fetchSlotsAsync = fetchSlotsAsync;
        _slotHintLabel = slotHintLabel;
        InitializeDatePicker();
    }

    // Legacy constructor for backward compatibility (Picker-based pages)
    public AppointmentSlotPickerHelper(
        DatePicker datePicker,
        Picker? slotPicker,
        Func<Task<int?>> getDoctorIdAsync,
        Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> fetchSlotsAsync,
        Label? slotHintLabel = null)
        : this(datePicker, getDoctorIdAsync, fetchSlotsAsync, slotHintLabel)
    {
        // Legacy: slotPicker is ignored, use new constructor instead
    }

    private void InitializeDatePicker()
    {
        _datePicker.DateSelected += async (_, _) => await ReloadSlotsAsync();
        _datePicker.MinimumDate = AppointmentScheduling.NormalizeToNextWeekday(DateTime.Today);
        if (_datePicker.Date is null || AppointmentScheduling.IsWeekend(_datePicker.Date.Value))
        {
            _datePicker.Date = _datePicker.MinimumDate;
        }
    }

    public void SetExcludeAppointmentId(int? id) => _excludeAppointmentId = id;

    public async Task InitializeAsync()
    {
        await ReloadSlotsAsync();
    }

    public DateTime? GetSelectedDateTime() => _selectedSlot?.ScheduledAt;

    public void SelectSlot(SlotViewModel slot)
    {
        if (!slot.IsAvailable)
        {
            return;
        }
        _selectedSlot = slot;
    }

    public async Task SelectExistingAsync(DateTime scheduledAt)
    {
        _datePicker.Date = scheduledAt.Date;
        await ReloadSlotsAsync(scheduledAt);
    }

    private async Task ReloadSlotsAsync(DateTime? preferTime = null)
    {
        var date = _datePicker.Date ?? DateTime.Today;
        if (AppointmentScheduling.IsWeekend(date))
        {
            date = AppointmentScheduling.NormalizeToNextWeekday(date);
            _datePicker.Date = date;
        }

        var doctorId = await _getDoctorIdAsync();
        Slots.Clear();
        _selectedSlot = null;

        if (doctorId is not > 0)
        {
            SetHint("Doktor seçildikten sonra uygun saatler listelenir.");
            return;
        }

        var slots = await _fetchSlotsAsync(doctorId.Value, date.Date, _excludeAppointmentId);
        var allSlots = slots.ToList();

        foreach (var s in allSlots)
        {
            var viewModel = new SlotViewModel
            {
                ScheduledAt = s.ScheduledAt,
                Label = s.Label,
                IsAvailable = s.IsAvailable
            };
            Slots.Add(viewModel);
        }

        var hasAvailable = allSlots.Any(s => s.IsAvailable);
        if (!hasAvailable)
        {
            SetHint("Bu tarihte boş randevu saati kalmadı. Başka bir gün seçin.");
            return;
        }

        SetHint("Hafta içi 08:30–16:30, öğle 12:00–13:00 hariç, 15 dk aralıklar. Gri slot'lar dolu randevu gösterir.");

        if (preferTime is { } preferred)
        {
            var slot = Slots.FirstOrDefault(s => s.ScheduledAt == preferred);
            if (slot is not null && slot.IsAvailable)
            {
                _selectedSlot = slot;
            }
        }
        else if (Slots.FirstOrDefault(s => s.IsAvailable) is { } firstAvailable)
        {
            _selectedSlot = firstAvailable;
        }
    }

    private void SetHint(string text)
    {
        if (_slotHintLabel is null)
        {
            return;
        }

        _slotHintLabel.Text = text;
        _slotHintLabel.IsVisible = true;
    }
}
