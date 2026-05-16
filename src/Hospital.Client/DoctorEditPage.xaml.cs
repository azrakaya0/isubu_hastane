using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class DoctorEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _doctorId;
    private readonly List<int> _clinicIds = new();

    public DoctorEditPage(int? doctorId)
    {
        InitializeComponent();
        _doctorId = doctorId;
        DeleteButton.IsVisible = doctorId is > 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var clinics = await _api.GetClinicsAsync(null);
                ClinicPicker.Items.Clear();
                _clinicIds.Clear();
                foreach (var c in clinics.OrderBy(x => x.Name))
                {
                    ClinicPicker.Items.Add(c.Name);
                    _clinicIds.Add(c.Id);
                }

                if (_doctorId is not > 0)
                {
                    if (ClinicPicker.Items.Count > 0)
                    {
                        ClinicPicker.SelectedIndex = 0;
                    }

                    return;
                }

                var doctors = await _api.GetDoctorsAsync(null, null);
                var d = doctors.FirstOrDefault(x => x.Id == _doctorId);
                if (d is null)
                {
                    await DisplayAlert("Hata", "Doktor bulunamadı.", "Tamam");
                    await Navigation.PopAsync();
                    return;
                }

                FirstNameEntry.Text = d.FirstName;
                LastNameEntry.Text = d.LastName;
                SpecialtyEntry.Text = d.Specialty;
                var idx = _clinicIds.IndexOf(d.ClinicId);
                ClinicPicker.SelectedIndex = idx >= 0 ? idx : 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        });
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var first = FirstNameEntry.Text?.Trim() ?? string.Empty;
        var last = LastNameEntry.Text?.Trim() ?? string.Empty;
        var spec = SpecialtyEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(spec))
        {
            await DisplayAlert("Doğrulama", "Ad, soyad ve uzmanlık zorunludur.", "Tamam");
            return;
        }

        if (ClinicPicker.SelectedIndex < 0 || ClinicPicker.SelectedIndex >= _clinicIds.Count)
        {
            await DisplayAlert("Doğrulama", "Poliklinik seçiniz.", "Tamam");
            return;
        }

        var clinicId = _clinicIds[ClinicPicker.SelectedIndex];
        var request = new CreateDoctorRequest
        {
            FirstName = first,
            LastName = last,
            Specialty = spec,
            ClinicId = clinicId
        };

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_doctorId is > 0)
                {
                    await _api.UpdateDoctorAsync(_doctorId.Value, new UpdateDoctorRequest
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Specialty = request.Specialty,
                        ClinicId = request.ClinicId
                    });
                }
                else
                {
                    await _api.CreateDoctorAsync(request);
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
        if (_doctorId is not > 0)
        {
            return;
        }

        if (!await DisplayAlert("Sil", "Bu doktoru silmek istediğinize emin misiniz?", "Evet", "Hayır"))
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await _api.DeleteDoctorAsync(_doctorId.Value);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sil", ex.Message, "Tamam");
            }
        });
    }
}
