using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class ClinicEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _clinicId;

    public ClinicEditPage(int? clinicId)
    {
        InitializeComponent();
        _clinicId = clinicId;
        DeleteButton.IsVisible = clinicId is > 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_clinicId is not > 0)
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var list = await _api.GetClinicsAsync(null);
                var c = list.FirstOrDefault(x => x.Id == _clinicId);
                if (c is null)
                {
                    await DisplayAlert("Hata", "Poliklinik bulunamadı.", "Tamam");
                    await Navigation.PopAsync();
                    return;
                }

                NameEntry.Text = c.Name;
                DescriptionEditor.Text = c.Description;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        });
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Doğrulama", "Poliklinik adı zorunludur.", "Tamam");
            return;
        }

        var desc = DescriptionEditor.Text?.Trim();
        var request = new CreateClinicRequest
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(desc) ? null : desc
        };

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_clinicId is > 0)
                {
                    await _api.UpdateClinicAsync(_clinicId.Value, new UpdateClinicRequest
                    {
                        Name = request.Name,
                        Description = request.Description
                    });
                }
                else
                {
                    await _api.CreateClinicAsync(request);
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
        if (_clinicId is not > 0)
        {
            return;
        }

        if (!await DisplayAlert("Sil", "Bu polikliniği silmek istediğinize emin misiniz?", "Evet", "Hayır"))
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await _api.DeleteClinicAsync(_clinicId.Value);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sil", ex.Message, "Tamam");
            }
        });
    }
}
