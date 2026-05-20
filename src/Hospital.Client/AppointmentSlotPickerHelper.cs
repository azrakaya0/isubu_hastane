using Hospital.Shared;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

/// <summary>
/// Randevu sayfalarında tarih ve 15 dk slot seçimini yönetir.
/// </summary>
internal sealed class AppointmentSlotPickerHelper
{
    private readonly DatePicker _datePicker;
    private readonly Picker _slotPicker;
    private readonly Label? _slotHintLabel;
    private readonly Func<Task<int?>> _getDoctorIdAsync;
    private readonly Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> _fetchSlotsAsync;
    private readonly List<DateTime> _slotTimes = new();
    private int? _excludeAppointmentId;

    public AppointmentSlotPickerHelper(
        DatePicker datePicker,
        Picker slotPicker,
        Func<Task<int?>> getDoctorIdAsync,
        Func<int, DateTime, int?, Task<IReadOnlyList<AppointmentSlotDto>>> fetchSlotsAsync,
        Label? slotHintLabel = null)
    {
        _datePicker = datePicker;
        _slotPicker = slotPicker;
        _getDoctorIdAsync = getDoctorIdAsync;
        _fetchSlotsAsync = fetchSlotsAsync;
        _slotHintLabel = slotHintLabel;
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

    public DateTime? GetSelectedDateTime()
    {
        if (_slotPicker.SelectedIndex < 0 || _slotPicker.SelectedIndex >= _slotTimes.Count)
        {
            return null;
        }

        return _slotTimes[_slotPicker.SelectedIndex];
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
        _slotTimes.Clear();
        _slotPicker.Items.Clear();
        _slotPicker.SelectedIndex = -1;

        if (doctorId is not > 0)
        {
            _slotPicker.Title = "Önce doktor seçin";
            SetHint("Doktor seçildikten sonra uygun saatler listelenir.");
            return;
        }

        var slots = await _fetchSlotsAsync(doctorId.Value, date.Date, _excludeAppointmentId);
        var available = slots.Where(s => s.IsAvailable).ToList();
        foreach (var s in available)
        {
            _slotTimes.Add(s.ScheduledAt);
            _slotPicker.Items.Add(s.Label);
        }

        if (_slotTimes.Count == 0)
        {
            _slotPicker.Title = "Uygun saat yok";
            SetHint("Bu tarihte boş randevu saati kalmadı veya gün kapalı. Başka bir gün seçin.");
            return;
        }

        _slotPicker.Title = "Saat seçin";
        SetHint("Hafta içi 08:30–16:30, öğle 12:00–13:00 hariç, 15 dk aralıklar.");

        if (preferTime is { } preferred)
        {
            var idx = _slotTimes.FindIndex(t => t == preferred);
            if (idx < 0)
            {
                _slotTimes.Add(preferred);
                _slotPicker.Items.Add(AppointmentScheduling.FormatSlotLabel(preferred));
                idx = _slotTimes.Count - 1;
            }

            _slotPicker.SelectedIndex = idx;
            return;
        }

        if (_slotTimes.Count > 0)
        {
            _slotPicker.SelectedIndex = 0;
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
