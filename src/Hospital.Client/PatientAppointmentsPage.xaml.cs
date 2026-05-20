using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class PatientAppointmentsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<AppointmentListRow> _items = new();
    private List<AppointmentDto> _snapshot = new();
    private readonly PickerFilterWatcher _statusPickerWatcher;

    public PatientAppointmentsPage()
    {
        InitializeComponent();
        ItemsCollection.ItemsSource = _items;
        RbAllDates.CheckedChanged += OnFilterChanged;
        RbToday.CheckedChanged += OnFilterChanged;
        RbWeek.CheckedChanged += OnFilterChanged;
        _statusPickerWatcher = new PickerFilterWatcher(StatusPicker, () =>
        {
            ApplyFilters();
            return Task.CompletedTask;
        });
        RbAllDates.IsChecked = true;
        StatusPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadWithSpinnerAsync();
    }

    private void OnFilterChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value != true)
        {
            return;
        }

        ApplyFilters();
    }

    private static DateTime WeekStartMonday(DateTime day)
    {
        var dow = day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
        return day.Date.AddDays(1 - dow);
    }

    private AppointmentStatus? GetLocalStatusFilter() =>
        StatusPicker.SelectedIndex switch
        {
            1 => AppointmentStatus.Scheduled,
            2 => AppointmentStatus.Completed,
            3 => AppointmentStatus.Cancelled,
            _ => null
        };

    private IEnumerable<AppointmentDto> FilterByDate(IEnumerable<AppointmentDto> rows)
    {
        if (RbToday.IsChecked == true)
        {
            var today = DateTime.Today;
            var end = today.AddDays(1);
            return rows.Where(a => a.ScheduledAt >= today && a.ScheduledAt < end);
        }

        if (RbWeek.IsChecked == true)
        {
            var start = WeekStartMonday(DateTime.Today);
            var end = start.AddDays(7);
            return rows.Where(a => a.ScheduledAt >= start && a.ScheduledAt < end);
        }

        return rows;
    }

    private void ApplyFilters()
    {
        var q = FilterByDate(_snapshot);
        var st = GetLocalStatusFilter();
        if (st.HasValue)
        {
            q = q.Where(a => a.Status == st.Value);
        }

        _items.Clear();
        foreach (var a in q.OrderByDescending(x => x.ScheduledAt))
        {
            _items.Add(new AppointmentListRow(a));
        }

        KpiTotal.Text = _items.Count.ToString();
        KpiScheduled.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Scheduled).ToString();
        KpiCompleted.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Completed).ToString();
        KpiCancelled.Text = _items.Count(x => x.Source.Status == AppointmentStatus.Cancelled).ToString();
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

    private async void OnPageErrorRetryClicked(object? sender, EventArgs e)
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        await ReloadWithSpinnerAsync();
    }

    private async Task LoadDataAsync()
    {
        await PageUi.HideListErrorAsync(PageErrorBanner);
        var list = await _api.GetPatientPortalAppointmentsAsync();
        _snapshot = list.ToList();
        ApplyFilters();
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
}
