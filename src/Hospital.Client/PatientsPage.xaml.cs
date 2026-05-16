using System.Collections.ObjectModel;
using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class PatientsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<PatientDto> _items = new();

    public PatientsPage()
    {
        InitializeComponent();
        PatientsCollection.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadWithSpinnerAsync();
    }

    private async Task LoadDataAsync()
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        var list = await _api.GetPatientsAsync(SearchEntry.Text, NationalIdEntry.Text);
        _items.Clear();
        foreach (var p in list.OrderByDescending(x => x.CreatedAt))
        {
            _items.Add(p);
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

    private async void OnSearchClicked(object? sender, EventArgs e) => await ReloadWithSpinnerAsync();

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
        await Navigation.PushAsync(new PatientEditPage(null));
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PatientsCollection.SelectedItem is not PatientDto p)
        {
            return;
        }

        PatientsCollection.SelectedItem = null;
        await Navigation.PushAsync(new PatientEditPage(p.Id));
    }
}
