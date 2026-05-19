using System.Collections.ObjectModel;
using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class DoctorsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<DoctorDto> _items = new();
    private readonly List<int?> _clinicFilterIds = new();
    private readonly DebouncedReload _searchDebouncer;
    private readonly PickerFilterWatcher _clinicPickerWatcher;

    public DoctorsPage()
    {
        InitializeComponent();
        DoctorsCollection.ItemsSource = _items;
        _searchDebouncer = new DebouncedReload(ReloadDoctorsWithSpinnerAsync);
        SearchFilterWiring.WireEntry(SearchEntry, _searchDebouncer);
        _clinicPickerWatcher = new PickerFilterWatcher(ClinicFilterPicker, ReloadDoctorsWithSpinnerAsync);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SearchEntry.Text = string.Empty;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await LoadClinicFilterAsync();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await PageUi.ShowListErrorAsync(PageErrorMessage, PageErrorBanner, ex.Message);
            }
        });
    }

    private async Task LoadClinicFilterAsync()
    {
        var clinics = await _api.GetClinicsAsync(null);
        using (_clinicPickerWatcher.SuppressChanges())
        {
            ClinicFilterPicker.Items.Clear();
            _clinicFilterIds.Clear();
            ClinicFilterPicker.Items.Add("Tümü");
            _clinicFilterIds.Add(null);
            foreach (var c in clinics.OrderBy(x => x.Name))
            {
                ClinicFilterPicker.Items.Add(c.Name);
                _clinicFilterIds.Add(c.Id);
            }

            if (ClinicFilterPicker.SelectedIndex < 0)
            {
                ClinicFilterPicker.SelectedIndex = 0;
            }
        }
    }

    private int? SelectedClinicFilter =>
        ClinicFilterPicker.SelectedIndex >= 0 && ClinicFilterPicker.SelectedIndex < _clinicFilterIds.Count
            ? _clinicFilterIds[ClinicFilterPicker.SelectedIndex]
            : null;

    private async Task LoadDataAsync()
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        var list = await _api.GetDoctorsAsync(SearchEntry.Text, SelectedClinicFilter);
        _items.Clear();
        foreach (var d in list.OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
        {
            _items.Add(d);
        }
    }

    private Task ReloadDoctorsWithSpinnerAsync() =>
        PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await PageUi.ShowListErrorAsync(PageErrorMessage, PageErrorBanner, ex.Message);
            }
        });

    private async void OnPageErrorRetryClicked(object? sender, EventArgs e)
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        await ReloadDoctorsWithSpinnerAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await PageUi.ShowListErrorAsync(PageErrorMessage, PageErrorBanner, ex.Message);
        }
        finally
        {
            RefreshHost.IsRefreshing = false;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DoctorEditPage(null));
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DoctorsCollection.SelectedItem is not DoctorDto d)
        {
            return;
        }

        DoctorsCollection.SelectedItem = null;
        await Navigation.PushAsync(new DoctorEditPage(d.Id));
    }
}
