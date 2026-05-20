using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class PatientBookAppointmentPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly List<ClinicDto> _clinics = new();
    private readonly List<DoctorDto> _doctors = new();
    private readonly PickerFilterWatcher _clinicPickerWatcher;
    private AppointmentSlotPickerHelper? _slotHelper;

    public PatientBookAppointmentPage()
    {
        InitializeComponent();
        _clinicPickerWatcher = new PickerFilterWatcher(ClinicPicker, LoadDoctorsForSelectedClinicAsync);
        DoctorPicker.SelectedIndexChanged += async (_, _) => await OnDoctorChangedAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ScheduleDatePicker.MinimumDate = DateTime.Today;
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            _clinics.Clear();
            var list = await _api.GetPatientPortalClinicsAsync(null);
            _clinics.AddRange(list.OrderBy(c => c.ClinicNumber ?? "zzz").ThenBy(c => c.Name));
            using (_clinicPickerWatcher.SuppressChanges())
            {
                ClinicPicker.Items.Clear();
                foreach (var c in _clinics)
                {
                    ClinicPicker.Items.Add(c.DisplayName);
                }
            }

            DoctorPicker.Items.Clear();
            DoctorPicker.Title = "Önce poliklinik seçin";
            _doctors.Clear();
            _slotHelper = new AppointmentSlotPickerHelper(
                ScheduleDatePicker,
                SlotPicker,
                () => Task.FromResult(GetSelectedDoctorId()),
                (doctorId, date, _) => _api.GetPatientPortalAppointmentSlotsAsync(doctorId, date),
                SlotHintLabel);
        });
    }

    private int? GetSelectedDoctorId() =>
        DoctorPicker.SelectedIndex >= 0 && DoctorPicker.SelectedIndex < _doctors.Count
            ? _doctors[DoctorPicker.SelectedIndex].Id
            : null;

    private async Task LoadDoctorsForSelectedClinicAsync()
    {
        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinics.Count)
        {
            return;
        }

        var clinic = _clinics[ClinicPicker.SelectedIndex];
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetPatientPortalDoctorsAsync(clinic.Id);
            _doctors.Clear();
            _doctors.AddRange(list.OrderBy(d => d.LastName));
            DoctorPicker.Items.Clear();
            foreach (var d in _doctors)
            {
                DoctorPicker.Items.Add($"{d.FirstName} {d.LastName} — {d.Specialty}");
            }

            DoctorPicker.Title = _doctors.Count > 0 ? "Doktor seçin" : "Bu birimde doktor yok";
            SlotPicker.Items.Clear();
            if (_doctors.Count > 0)
            {
                DoctorPicker.SelectedIndex = 0;
                await OnDoctorChangedAsync();
            }
        });
    }

    private async Task OnDoctorChangedAsync()
    {
        if (_slotHelper is null)
        {
            return;
        }

        await _slotHelper.InitializeAsync();
    }

    private async void OnBookClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinics.Count)
        {
            ErrorLabel.Text = "Poliklinik seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (DoctorPicker.SelectedIndex < 0 || DoctorPicker.SelectedIndex >= _doctors.Count)
        {
            ErrorLabel.Text = "Doktor seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var when = _slotHelper?.GetSelectedDateTime();
        if (when is null)
        {
            ErrorLabel.Text = "Uygun bir randevu saati seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (when.Value < DateTime.Now.AddMinutes(-5))
        {
            ErrorLabel.Text = "Geçmiş bir saat seçilemez.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var clinic = _clinics[ClinicPicker.SelectedIndex];
        var doctor = _doctors[DoctorPicker.SelectedIndex];
        var request = new PortalPatientBookRequest
        {
            ClinicId = clinic.Id,
            DoctorId = doctor.Id,
            ScheduledAt = when.Value,
            Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim()
        };

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var created = await _api.PatientPortalBookAppointmentAsync(request);
                if (created is null)
                {
                    ErrorLabel.Text = "Randevu oluşturulamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                await DisplayAlert(
                    "Randevu",
                    $"Randevunuz kaydedildi.\n{created.DoctorFullName} — {created.ScheduledAt:dd.MM.yyyy HH:mm}\nSeçtiğiniz doktorun Randevularım listesinde görünür.",
                    "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }
}
