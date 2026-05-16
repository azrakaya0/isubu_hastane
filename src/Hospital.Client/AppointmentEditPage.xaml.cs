using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class AppointmentEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _appointmentId;
    private readonly List<int> _patientIds = new();
    private readonly List<int> _clinicIds = new();
    private readonly List<int> _doctorIds = new();
    private List<DoctorDto> _allDoctors = new();

    public AppointmentEditPage(int? appointmentId)
    {
        InitializeComponent();
        _appointmentId = appointmentId;
        DeleteButton.IsVisible = appointmentId is > 0;
        StatusPicker.Items.Add("Planlandı");
        StatusPicker.Items.Add("Tamamlandı");
        StatusPicker.Items.Add("İptal");
        StatusPicker.SelectedIndex = 0;
        ClinicPicker.SelectedIndexChanged += OnClinicChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await LoadPickersAsync();

                if (_appointmentId is not > 0)
                {
                    ScheduleDatePicker.Date = DateTime.Today;
                    ScheduleTimePicker.Time = new TimeSpan(9, 0, 0);
                    if (_patientIds.Count > 0)
                    {
                        PatientPicker.SelectedIndex = 0;
                    }

                    if (_clinicIds.Count > 0)
                    {
                        ClinicPicker.SelectedIndex = 0;
                        await FilterDoctorsByClinicAsync(_clinicIds[0]);
                    }

                    return;
                }

                var a = await _api.GetAppointmentAsync(_appointmentId.Value);
                if (a is null)
                {
                    await DisplayAlert("Hata", "Randevu bulunamadı.", "Tamam");
                    await Navigation.PopAsync();
                    return;
                }

                SelectPickerIndex(PatientPicker, _patientIds, a.PatientId);
                ClinicPicker.SelectedIndexChanged -= OnClinicChanged;
                SelectPickerIndex(ClinicPicker, _clinicIds, a.ClinicId);
                await FilterDoctorsByClinicAsync(a.ClinicId);
                SelectPickerIndex(DoctorPicker, _doctorIds, a.DoctorId);
                ClinicPicker.SelectedIndexChanged += OnClinicChanged;
                ScheduleDatePicker.Date = a.ScheduledAt.Date;
                ScheduleTimePicker.Time = a.ScheduledAt.TimeOfDay;
                StatusPicker.SelectedIndex = a.Status switch
                {
                    AppointmentStatus.Scheduled => 0,
                    AppointmentStatus.Completed => 1,
                    AppointmentStatus.Cancelled => 2,
                    _ => 0
                };
                NotesEditor.Text = StripUrgentPrefix(a.Notes);
                UrgentSwitch.IsToggled = a.Notes?.StartsWith("[Acil] ", StringComparison.Ordinal) == true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        });
    }

    private static string? StripUrgentPrefix(string? notes)
    {
        if (string.IsNullOrEmpty(notes))
        {
            return notes;
        }

        return notes.StartsWith("[Acil] ", StringComparison.Ordinal) ? notes.Substring(7) : notes;
    }

    private static void SelectPickerIndex(Picker picker, List<int> ids, int id)
    {
        var idx = ids.IndexOf(id);
        picker.SelectedIndex = idx >= 0 ? idx : -1;
    }

    private async Task LoadPickersAsync()
    {
        var patients = await _api.GetPatientsAsync(null, null);
        PatientPicker.Items.Clear();
        _patientIds.Clear();
        foreach (var p in patients.OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
        {
            PatientPicker.Items.Add($"{p.FirstName} {p.LastName}");
            _patientIds.Add(p.Id);
        }

        var clinics = await _api.GetClinicsAsync(null);
        ClinicPicker.Items.Clear();
        _clinicIds.Clear();
        foreach (var c in clinics.OrderBy(x => x.Name))
        {
            ClinicPicker.Items.Add(c.Name);
            _clinicIds.Add(c.Id);
        }

        _allDoctors = (await _api.GetDoctorsAsync(null, null)).ToList();
    }

    private void OnClinicChanged(object? sender, EventArgs e)
    {
        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinicIds.Count)
        {
            return;
        }

        _ = FilterDoctorsByClinicAsync(_clinicIds[ClinicPicker.SelectedIndex]);
    }

    private Task FilterDoctorsByClinicAsync(int clinicId)
    {
        var filtered = _allDoctors.Where(d => d.ClinicId == clinicId).OrderBy(d => d.LastName).ToList();
        DoctorPicker.Items.Clear();
        _doctorIds.Clear();
        foreach (var d in filtered)
        {
            DoctorPicker.Items.Add($"{d.FirstName} {d.LastName} — {d.Specialty}");
            _doctorIds.Add(d.Id);
        }

        if (DoctorPicker.Items.Count > 0)
        {
            DoctorPicker.SelectedIndex = 0;
        }

        return Task.CompletedTask;
    }

    private AppointmentStatus SelectedStatus => StatusPicker.SelectedIndex switch
    {
        1 => AppointmentStatus.Completed,
        2 => AppointmentStatus.Cancelled,
        _ => AppointmentStatus.Scheduled
    };

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (PatientPicker.SelectedIndex < 0 || PatientPicker.SelectedIndex >= _patientIds.Count)
        {
            await DisplayAlert("Doğrulama", "Hasta seçiniz.", "Tamam");
            return;
        }

        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinicIds.Count)
        {
            await DisplayAlert("Doğrulama", "Poliklinik seçiniz.", "Tamam");
            return;
        }

        if (DoctorPicker.SelectedIndex < 0 || DoctorPicker.SelectedIndex >= _doctorIds.Count)
        {
            await DisplayAlert("Doğrulama", "Bu poliklinik için doktor bulunamadı veya seçilmedi.", "Tamam");
            return;
        }

        var patientId = _patientIds[PatientPicker.SelectedIndex];
        var clinicId = _clinicIds[ClinicPicker.SelectedIndex];
        var doctorId = _doctorIds[DoctorPicker.SelectedIndex];
        var when = ScheduleDatePicker.Date.Date + ScheduleTimePicker.Time;
        var notes = NotesEditor.Text?.Trim();
        if (UrgentSwitch.IsToggled)
        {
            notes = string.IsNullOrWhiteSpace(notes) ? "[Acil] " : "[Acil] " + notes;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_appointmentId is > 0)
                {
                    await _api.UpdateAppointmentAsync(_appointmentId.Value, new UpdateAppointmentRequest
                    {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        ClinicId = clinicId,
                        ScheduledAt = when,
                        Status = SelectedStatus,
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                    });
                }
                else
                {
                    await _api.CreateAppointmentAsync(new CreateAppointmentRequest
                    {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        ClinicId = clinicId,
                        ScheduledAt = when,
                        Status = SelectedStatus,
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                    });
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Kayıt", ex.Message, "Tamam");
            }
        });
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_appointmentId is not > 0)
        {
            return;
        }

        if (!await DisplayAlert("Sil", "Bu randevuyu silmek istediğinize emin misiniz?", "Evet", "Hayır"))
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await _api.DeleteAppointmentAsync(_appointmentId.Value);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sil", ex.Message, "Tamam");
            }
        });
    }
}
