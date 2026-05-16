using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class DoctorDutyEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _dutyId;
    private readonly List<DoctorDto> _doctors = new();
    private readonly List<ClinicDto> _clinics = new();

    public DoctorDutyEditPage(int? dutyId)
    {
        InitializeComponent();
        _dutyId = dutyId;
        DeleteButton.IsVisible = dutyId.HasValue;
        if (dutyId.HasValue)
        {
            HeroTitle.Text = "Kaydı düzenle";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var doctors = (await _api.GetDoctorsAsync(null, null)).OrderBy(d => d.LastName).ToList();
            _doctors.Clear();
            _doctors.AddRange(doctors);
            DoctorPicker.Items.Clear();
            foreach (var d in _doctors)
            {
                DoctorPicker.Items.Add($"{d.FirstName} {d.LastName} — {d.Specialty}");
            }

            var clinics = (await _api.GetClinicsAsync(null)).OrderBy(c => c.Name).ToList();
            _clinics.Clear();
            _clinics.AddRange(clinics);
            ClinicPicker.Items.Clear();
            foreach (var c in _clinics)
            {
                ClinicPicker.Items.Add(c.Name);
            }

            if (_dutyId is { } id)
            {
                var existing = await _api.GetDoctorDutyAsync(id);
                if (existing is null)
                {
                    ErrorLabel.Text = "Kayıt bulunamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                var di = _doctors.FindIndex(d => d.Id == existing.DoctorId);
                DoctorPicker.SelectedIndex = di >= 0 ? di : -1;
                var ci = _clinics.FindIndex(c => c.Id == existing.ClinicId);
                ClinicPicker.SelectedIndex = ci >= 0 ? ci : -1;
                DutyDatePicker.Date = existing.DutyDate.Date;
                KindEntry.Text = existing.DutyKind;
                NotesEditor.Text = existing.Notes;
            }
        });
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (DoctorPicker.SelectedIndex < 0 || DoctorPicker.SelectedIndex >= _doctors.Count)
        {
            ErrorLabel.Text = "Doktor seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinics.Count)
        {
            ErrorLabel.Text = "Poliklinik seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var kind = (KindEntry.Text ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(kind))
        {
            ErrorLabel.Text = "Tür alanı zorunludur.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var doctor = _doctors[DoctorPicker.SelectedIndex];
        var clinic = _clinics[ClinicPicker.SelectedIndex];
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_dutyId is null)
                {
                    var create = new CreateDoctorDutyRequest
                    {
                        DoctorId = doctor.Id,
                        ClinicId = clinic.Id,
                        DutyDate = DutyDatePicker.Date,
                        DutyKind = kind,
                        Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim()
                    };

                    var created = await _api.CreateDoctorDutyAsync(create);
                    if (created is null)
                    {
                        ErrorLabel.Text = "Kayıt oluşturulamadı.";
                        ErrorLabel.IsVisible = true;
                        return;
                    }

                    await DisplayAlert("Kayıt", "Plan kaydedildi.", "Tamam");
                }
                else
                {
                    var update = new UpdateDoctorDutyRequest
                    {
                        DoctorId = doctor.Id,
                        ClinicId = clinic.Id,
                        DutyDate = DutyDatePicker.Date,
                        DutyKind = kind,
                        Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim()
                    };

                    await _api.UpdateDoctorDutyAsync(_dutyId.Value, update);
                    await DisplayAlert("Kayıt", "Güncellendi.", "Tamam");
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_dutyId is null)
        {
            return;
        }

        var ok = await DisplayAlert("Sil", "Bu kaydı silmek istiyor musunuz?", "Evet", "Hayır");
        if (!ok)
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            await _api.DeleteDoctorDutyAsync(_dutyId.Value);
            await Navigation.PopAsync();
        });
    }
}
