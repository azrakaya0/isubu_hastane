using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Shared;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

/// <summary>
/// Randevu sayfalarında tarih ve 15 dk slot seçimini yönetir.
/// İki mod: Picker (admin) ve FlexLayout/callback (hasta portalı).
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
    private Action? _onSlotsReloaded;

    // Picker modu için
    private readonly Picker? _slotPicker;
    private readonly List<SlotViewModel> _pickerSlots = new();

    // Yeni mod: FlexLayout / callback tabanlı sayfalar için
    public AppointmentSlotPickerHelper(
        DatePicker datePicker,
        Func<Task<int?>> getDoctorIdAsync,
        Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> fetchSlotsAsync,
        Label? slotHintLabel = null,
        Action? onSlotsReloaded = null)
    {
        _datePicker = datePicker;
        _getDoctorIdAsync = getDoctorIdAsync;
        _fetchSlotsAsync = fetchSlotsAsync;
        _slotHintLabel = slotHintLabel;
        _onSlotsReloaded = onSlotsReloaded;
        InitializeDatePicker();
    }

    // Picker modu: admin randevu sayfası için
    public AppointmentSlotPickerHelper(
        DatePicker datePicker,
        Picker? slotPicker,
        Func<Task<int?>> getDoctorIdAsync,
        Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> fetchSlotsAsync,
        Label? slotHintLabel = null)
        : this(datePicker, getDoctorIdAsync, fetchSlotsAsync, slotHintLabel)
    {
        _slotPicker = slotPicker;
        if (_slotPicker is not null)
            _slotPicker.SelectedIndexChanged += OnPickerSelectionChanged;
    }

    private void OnPickerSelectionChanged(object? sender, EventArgs e)
    {
        if (_slotPicker is null) return;
        var idx = _slotPicker.SelectedIndex;
        _selectedSlot = (idx >= 0 && idx < _pickerSlots.Count) ? _pickerSlots[idx] : null;
    }

    private void InitializeDatePicker()
    {
        _datePicker.DateSelected += async (_, _) => await ReloadSlotsAsync();
        _datePicker.MinimumDate = AppointmentScheduling.NormalizeToNextWeekday(DateTime.Today);
        if (_datePicker.Date is null || AppointmentScheduling.IsWeekend(_datePicker.Date.Value))
            _datePicker.Date = _datePicker.MinimumDate;
    }

    public void SetExcludeAppointmentId(int? id) => _excludeAppointmentId = id;

    public async Task InitializeAsync() => await ReloadSlotsAsync();

    public DateTime? GetSelectedDateTime() => _selectedSlot?.ScheduledAt;

    public void SelectSlot(SlotViewModel slot)
    {
        if (!slot.IsAvailable) return;
        _selectedSlot = slot;
    }

    public async Task SelectExistingAsync(DateTime scheduledAt)
    {
        _datePicker.Date = scheduledAt.Date;
        await ReloadSlotsAsync(scheduledAt);
    }

    private async Task ReloadSlotsAsync(DateTime? preferTime = null)
    {
        var date = (_datePicker.Date ?? DateTime.Today).Date;
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
            ClearPicker();
            return;
        }

        var slots = await _fetchSlotsAsync(doctorId.Value, date, _excludeAppointmentId);
        var allSlots = slots.ToList();

        foreach (var s in allSlots)
        {
            Slots.Add(new SlotViewModel
            {
                ScheduledAt = s.ScheduledAt,
                Label       = s.Label,
                IsAvailable = s.IsAvailable
            });
        }

        var hasAvailable = allSlots.Any(s => s.IsAvailable);
        if (!hasAvailable)
        {
            SetHint("Bu tarihte boş randevu saati kalmadı. Başka bir gün seçin.");
            ClearPicker();
            return;
        }

        SetHint("Hafta içi 08:30–16:30, öğle 12:00–13:00 hariç, 15 dk aralıklar.");

        // Picker modunu doldur
        if (_slotPicker is not null)
        {
            _pickerSlots.Clear();
            _slotPicker.Items.Clear();
            foreach (var s in Slots.Where(s => s.IsAvailable))
            {
                _slotPicker.Items.Add(s.Label);
                _pickerSlots.Add(s);
            }

            if (preferTime is { } preferred)
            {
                var idx = _pickerSlots.FindIndex(s => s.ScheduledAt == preferred);
                _slotPicker.SelectedIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                _slotPicker.SelectedIndex = _pickerSlots.Count > 0 ? 0 : -1;
            }

            if (_slotPicker.SelectedIndex >= 0 && _slotPicker.SelectedIndex < _pickerSlots.Count)
                _selectedSlot = _pickerSlots[_slotPicker.SelectedIndex];
        }
        else
        {
            // FlexLayout / callback modu
            if (preferTime is { } preferred)
            {
                var slot = Slots.FirstOrDefault(s => s.ScheduledAt == preferred);
                if (slot is not null && slot.IsAvailable)
                    _selectedSlot = slot;
            }
            else if (Slots.FirstOrDefault(s => s.IsAvailable) is { } firstAvailable)
            {
                _selectedSlot = firstAvailable;
            }

            _onSlotsReloaded?.Invoke();
        }
    }

    private void ClearPicker()
    {
        if (_slotPicker is null) return;
        _pickerSlots.Clear();
        _slotPicker.Items.Clear();
        _slotPicker.SelectedIndex = -1;
    }

    private void SetHint(string text)
    {
        if (_slotHintLabel is null) return;
        _slotHintLabel.Text = text;
        _slotHintLabel.IsVisible = true;
    }
}
