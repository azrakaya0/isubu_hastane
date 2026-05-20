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
    private readonly PickerFilterWatcher _clinicPickerWatcher;
    private AppointmentSlotPickerHelper? _slotHelper;

    public AppointmentEditPage(int? appointmentId)
    {
        InitializeComponent();
        _appointmentId = appointmentId;
        DeleteButton.IsVisible = appointmentId is > 0;
        StatusPicker.Items.Add("Planlandı");
        StatusPicker.Items.Add("Tamamlandı");
        StatusPicker.Items.Add("İptal");
        StatusPicker.SelectedIndex = 0;
        _clinicPickerWatcher = new PickerFilterWatcher(ClinicPicker, OnClinicFilterCommittedAsync);
        DoctorPicker.SelectedIndexChanged += async (_, _) => await ReloadSlotsAsync();
        NewPatientSwitch.Toggled += (_, _) => ToggleNewPatientPanel();
    }

    private void ToggleNewPatientPanel()
    {
        var isNew = NewPatientSwitch.IsToggled;
        PatientPickerBorder.IsVisible = !isNew;
        NewPatientPanel.IsVisible = isNew;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await LoadPickersAsync();
                _slotHelper = new AppointmentSlotPickerHelper(
                    ScheduleDatePicker,
                    SlotPicker,
                    () => Task.FromResult(GetSelectedDoctorId()),
                    (doctorId, date, exclude) => _api.GetAppointmentSlotsAsync(doctorId, date, exclude),
                    SlotHintLabel);
                _slotHelper.SetExcludeAppointmentId(_appointmentId);

                if (_appointmentId is not > 0)
                {
                    ScheduleDatePicker.Date = DateTime.Today;
                    if (_patientIds.Count > 0)
                    {
                        PatientPicker.SelectedIndex = 0;
                    }

                    if (_clinicIds.Count > 0)
                    {
                        ClinicPicker.SelectedIndex = 0;
                        await FilterDoctorsByClinicAsync(_clinicIds[0]);
                        await ReloadSlotsAsync();
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
                using (_clinicPickerWatcher.SuppressChanges())
                {
                    SelectPickerIndex(ClinicPicker, _clinicIds, a.ClinicId);
                }

                await FilterDoctorsByClinicAsync(a.ClinicId);
                SelectPickerIndex(DoctorPicker, _doctorIds, a.DoctorId);
                await _slotHelper.SelectExistingAsync(a.ScheduledAt);
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
        foreach (var c in clinics.OrderBy(x => x.ClinicNumber ?? "zzz").ThenBy(x => x.Name))
        {
            ClinicPicker.Items.Add(c.DisplayName);
            _clinicIds.Add(c.Id);
        }

        _allDoctors = (await _api.GetDoctorsAsync(null, null)).ToList();
    }

    private Task OnClinicFilterCommittedAsync()
    {
        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinicIds.Count)
        {
            return Task.CompletedTask;
        }

        return FilterDoctorsByClinicAsync(_clinicIds[ClinicPicker.SelectedIndex]);
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

        return ReloadSlotsAsync();
    }

    private int? GetSelectedDoctorId() =>
        DoctorPicker.SelectedIndex >= 0 && DoctorPicker.SelectedIndex < _doctorIds.Count
            ? _doctorIds[DoctorPicker.SelectedIndex]
            : null;

    private Task ReloadSlotsAsync() =>
        _slotHelper?.InitializeAsync() ?? Task.CompletedTask;

    private AppointmentStatus SelectedStatus => StatusPicker.SelectedIndex switch
    {
        1 => AppointmentStatus.Completed,
        2 => AppointmentStatus.Cancelled,
        _ => AppointmentStatus.Scheduled
    };

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var useNewPatient = NewPatientSwitch.IsToggled;
        if (!useNewPatient && (PatientPicker.SelectedIndex < 0 || PatientPicker.SelectedIndex >= _patientIds.Count))
        {
            await DisplayAlert("Doğrulama", "Hasta seçiniz veya yeni hasta seçeneğini açın.", "Tamam");
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

        var patientId = useNewPatient ? 0 : _patientIds[PatientPicker.SelectedIndex];
        AppointmentWalkInPatientRequest? walkIn = null;
        if (useNewPatient)
        {
            walkIn = new AppointmentWalkInPatientRequest
            {
                FirstName = NewPatientFirstNameEntry.Text?.Trim() ?? string.Empty,
                LastName = NewPatientLastNameEntry.Text?.Trim() ?? string.Empty,
                NationalId = NewPatientNationalIdEntry.Text?.Trim(),
                Phone = NewPatientPhoneEntry.Text?.Trim()
            };
            if (string.IsNullOrWhiteSpace(walkIn.FirstName) || string.IsNullOrWhiteSpace(walkIn.LastName))
            {
                await DisplayAlert("Doğrulama", "Yeni hasta için ad ve soyad giriniz.", "Tamam");
                return;
            }
        }

        var clinicId = _clinicIds[ClinicPicker.SelectedIndex];
        var doctorId = _doctorIds[DoctorPicker.SelectedIndex];
        var when = _slotHelper?.GetSelectedDateTime();
        if (when is null)
        {
            await DisplayAlert("Doğrulama", "Uygun bir randevu saati seçiniz.", "Tamam");
            return;
        }
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
                        ScheduledAt = when.Value,
                        Status = SelectedStatus,
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                    });
                }
                else
                {
                    await _api.CreateAppointmentAsync(new CreateAppointmentRequest
                    {
                        PatientId = patientId,
                        NewPatient = walkIn,
                        DoctorId = doctorId,
                        ClinicId = clinicId,
                        ScheduledAt = when.Value,
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
