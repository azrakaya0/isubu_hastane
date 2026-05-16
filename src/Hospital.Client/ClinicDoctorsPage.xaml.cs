using System.Collections.ObjectModel;
using Hospital.Client.Services;
using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class ClinicDoctorsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ClinicDto _clinic;
    private readonly ObservableCollection<DoctorDto> _items = new();

    public ClinicDoctorsPage(ClinicDto clinic)
    {
        InitializeComponent();
        _clinic = clinic;
        Title = clinic.Name;
        ClinicTitleLabel.Text = clinic.Name;
        ClinicDescLabel.Text = string.IsNullOrWhiteSpace(clinic.Description)
            ? "Bu birim için açıklama girilmemiş."
            : clinic.Description;
        DoctorsCollection.ItemsSource = _items;

        var thumb = ClinicImageMapper.TryResolveImage(clinic.Name);
        if (!string.IsNullOrEmpty(thumb))
        {
            ClinicHeroImage.Source = ImageSource.FromFile(thumb);
            ClinicHeroImage.IsVisible = true;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync() =>
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetClinicDoctorsAsync(_clinic.Id);
            _items.Clear();
            foreach (var d in list.OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            {
                _items.Add(d);
            }
        });

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await ReloadAsync();
        }
        finally
        {
            RefreshHost.IsRefreshing = false;
        }
    }

    private async void OnEditClinicClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new ClinicEditPage(_clinic.Id));
}
