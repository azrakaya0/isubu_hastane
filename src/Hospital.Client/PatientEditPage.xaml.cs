using System.Text.RegularExpressions;
using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class PatientEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _patientId;

    public PatientEditPage(int? patientId)
    {
        InitializeComponent();
        _patientId = patientId;
        DeleteButton.IsVisible = patientId is > 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_patientId is not > 0)
        {
            BirthDatePicker.Date = DateTime.Today.AddYears(-25);
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var p = await _api.GetPatientAsync(_patientId.Value);
                if (p is null)
                {
                    await DisplayAlert("Hata", "Hasta bulunamadı.", "Tamam");
                    await Navigation.PopAsync();
                    return;
                }

                FirstNameEntry.Text = p.FirstName;
                LastNameEntry.Text = p.LastName;
                NationalIdEntry.Text = p.NationalId;
                PhoneEntry.Text = p.Phone;
                EmailEntry.Text = p.Email;
                BirthDatePicker.Date = p.BirthDate.Date;
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
        var national = NationalIdEntry.Text?.Trim() ?? string.Empty;
        var phone = PhoneEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
        {
            await DisplayAlert("Doğrulama", "Ad ve soyad zorunludur.", "Tamam");
            return;
        }

        if (national.Length != 11 || !Regex.IsMatch(national, "^[0-9]{11}$"))
        {
            await DisplayAlert("Doğrulama", "T.C. kimlik 11 rakamdan oluşmalıdır.", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            await DisplayAlert("Doğrulama", "Telefon zorunludur.", "Tamam");
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
        {
            await DisplayAlert("Doğrulama", "Geçerli bir e-posta giriniz.", "Tamam");
            return;
        }

        var request = new CreatePatientRequest
        {
            FirstName = first,
            LastName = last,
            NationalId = national,
            Phone = phone,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            BirthDate = BirthDatePicker.Date!.Value
        };

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_patientId is > 0)
                {
                    await _api.UpdatePatientAsync(_patientId.Value, new UpdatePatientRequest
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        NationalId = request.NationalId,
                        Phone = request.Phone,
                        Email = request.Email,
                        BirthDate = request.BirthDate
                    });
                }
                else
                {
                    await _api.CreatePatientAsync(request);
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
        if (_patientId is not > 0)
        {
            return;
        }

        if (!await DisplayAlert("Sil", "Bu hastayı silmek istediğinize emin misiniz?", "Evet", "Hayır"))
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await _api.DeletePatientAsync(_patientId.Value);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sil", ex.Message, "Tamam");
            }
        });
    }
}
