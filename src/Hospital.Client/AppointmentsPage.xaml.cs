using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class AppointmentsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<AppointmentListRow> _items = new();

    public AppointmentsPage()
    {
        InitializeComponent();
        AppointmentsCollection.ItemsSource = _items;
        RbAll.CheckedChanged += OnRangeOrStatusFilterChanged;
        RbToday.CheckedChanged += OnRangeOrStatusFilterChanged;
        RbWeek.CheckedChanged += OnRangeOrStatusFilterChanged;
        StatusFilterPicker.SelectedIndexChanged += (_, _) =>
        {
            _ = ReloadWithSpinnerAsync();
        };
        RbAll.IsChecked = true;
        StatusFilterPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadWithSpinnerAsync();
    }

    private async void OnRangeOrStatusFilterChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value != true)
        {
            return;
        }

        await ReloadWithSpinnerAsync();
    }

    private static DateTime WeekStartMonday(DateTime day)
    {
        var dow = day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
        return day.Date.AddDays(1 - dow);
    }

    private (DateTime? From, DateTime? To) GetDateRange()
    {
        if (RbToday.IsChecked == true)
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(1).AddTicks(-1);
            return (start, end);
        }

        if (RbWeek.IsChecked == true)
        {
            var start = WeekStartMonday(DateTime.Today);
            var end = start.AddDays(7).AddTicks(-1);
            return (start, end);
        }

        return (null, null);
    }

    private AppointmentStatus? GetStatusFilter()
    {
        return StatusFilterPicker.SelectedIndex switch
        {
            1 => AppointmentStatus.Scheduled,
            2 => AppointmentStatus.Completed,
            3 => AppointmentStatus.Cancelled,
            _ => null
        };
    }

    private async Task LoadDataAsync()
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        var range = GetDateRange();
        var status = GetStatusFilter();
        var list = await _api.GetAppointmentsAsync(null, null, null, range.From, range.To, status);
        _items.Clear();
        foreach (var a in list.OrderByDescending(x => x.ScheduledAt))
        {
            _items.Add(new AppointmentListRow(a));
        }

        UpdateKpis();
    }

    private void UpdateKpis()
    {
        KpiTotal.Text = _items.Count.ToString();
        KpiScheduled.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Scheduled).ToString();
        KpiCompleted.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Completed).ToString();
        KpiCancelled.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Cancelled).ToString();
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
        await Navigation.PushAsync(new AppointmentEditPage(null));
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AppointmentsCollection.SelectedItem is not AppointmentListRow row)
        {
            return;
        }

        AppointmentsCollection.SelectedItem = null;
        await Navigation.PushAsync(new AppointmentEditPage(row.Source.Id));
    }
}
