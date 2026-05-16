using System.Collections.ObjectModel;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class PatientTimelinePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<TimelineEntryDto> _items = new();

    public PatientTimelinePage()
    {
        InitializeComponent();
        TimelineCollection.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync() =>
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetPatientPortalTimelineAsync();
            _items.Clear();
            foreach (var x in list.OrderByDescending(i => i.At))
            {
                _items.Add(x);
            }
        });

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await LoadAsync();
        }
        finally
        {
            RefreshHost.IsRefreshing = false;
        }
    }
}
