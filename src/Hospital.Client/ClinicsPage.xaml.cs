using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class ClinicsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<ClinicListRow> _items = new();
    private string? _lastSearch;

    private readonly DebouncedReload _searchDebouncer;

    public ClinicsPage()
    {
        InitializeComponent();
        ClinicsCollection.ItemsSource = _items;
        _searchDebouncer = new DebouncedReload(ReloadWithSpinnerAsync);
        SearchFilterWiring.WireSearchBar(ClinicSearch, _searchDebouncer);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _lastSearch = null;
        ClinicSearch.Text = string.Empty;
        await ReloadWithSpinnerAsync();
    }

    private async Task LoadDataAsync()
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        _lastSearch = string.IsNullOrWhiteSpace(ClinicSearch.Text) ? null : ClinicSearch.Text.Trim();
        var list = await _api.GetClinicsAsync(_lastSearch);
        _items.Clear();
        foreach (var c in list.OrderBy(x => x.Name))
        {
            _items.Add(new ClinicListRow(c));
        }
    }

    private Task ReloadWithSpinnerAsync() =>
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
        await ReloadWithSpinnerAsync();
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
        await Navigation.PushAsync(new ClinicEditPage(null));
    }

    private async void OnClinicNameTapped(object? sender, TappedEventArgs e)
    {
        var dto = e.Parameter switch
        {
            ClinicDto d => d,
            ClinicListRow r => r.Source,
            _ => null
        };

        if (dto is null)
        {
            return;
        }

        await Navigation.PushAsync(new ClinicDoctorsPage(dto));
    }

    private async void OnClinicEditTapped(object? sender, TappedEventArgs e)
    {
        var dto = e.Parameter switch
        {
            ClinicDto d => d,
            ClinicListRow r => r.Source,
            _ => null
        };

        if (dto is null)
        {
            return;
        }

        await Navigation.PushAsync(new ClinicEditPage(dto.Id));
    }
}
