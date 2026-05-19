using System.Collections.ObjectModel;
using System.Globalization;
using Hospital.Shared.Dtos;
using Hospital.Client.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Hospital.Client;

public partial class DoctorCalendarPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<AppointmentDto> _dayAppointments = new();
    private List<AppointmentDto> _allAppointments = new();
    private DateTime _selectedDate = DateTime.Today;
    private readonly CultureInfo _culture = new("tr-TR");

    public DoctorCalendarPage()
    {
        InitializeComponent();
        DayAppointmentsCollection.ItemsSource = _dayAppointments;
        DayPicker.Date = _selectedDate;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BuildDayChips();
        await ReloadWithSpinnerAsync();
    }

    private void BuildDayChips()
    {
        DayChipsHost.Children.Clear();
        var start = DateTime.Today.AddDays(-3);
        for (var i = 0; i < 18; i++)
        {
            var d = start.AddDays(i);
            DayChipsHost.Children.Add(CreateDayChip(d));
        }
    }

    private View CreateDayChip(DateTime d)
    {
        var isSelected = d.Date == _selectedDate.Date;
        var border = new Border
        {
            Padding = new Thickness(12, 8),
            BackgroundColor = isSelected
                ? Color.FromArgb("#1565C0")
                : Color.FromArgb("#FFFFFF"),
            Stroke = Color.FromArgb("#C5D9ED"),
            StrokeThickness = 1
        };
        border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) };
        var stack = new VerticalStackLayout { Spacing = 2 };
        stack.Children.Add(new Label
        {
            Text = d.ToString("ddd", _culture),
            FontSize = 11,
            TextColor = isSelected ? Colors.White : Color.FromArgb("#5A7DA1"),
            HorizontalTextAlignment = TextAlignment.Center
        });
        stack.Children.Add(new Label
        {
            Text = d.ToString("d MMM", _culture),
            FontSize = 13,
            FontFamily = "OpenSansSemibold",
            TextColor = isSelected ? Colors.White : Color.FromArgb("#1E3A5F"),
            HorizontalTextAlignment = TextAlignment.Center
        });
        border.Content = stack;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            _selectedDate = d.Date;
            DayPicker.Date = d.Date;
            BuildDayChips();
            ApplyFilter();
            UpdateSummary();
        };
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private void OnDayPickerDateSelected(object? sender, DateChangedEventArgs e)
    {
        _selectedDate = e.NewDate!.Value.Date;
        BuildDayChips();
        ApplyFilter();
        UpdateSummary();
    }

    private void OnTodayClicked(object? sender, EventArgs e)
    {
        _selectedDate = DateTime.Today;
        DayPicker.Date = _selectedDate;
        BuildDayChips();
        ApplyFilter();
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        SelectedSummaryLabel.Text =
            $"{_selectedDate.ToString("D", _culture)} · {_dayAppointments.Count} randevu";
    }

    private void ApplyFilter()
    {
        _dayAppointments.Clear();
        foreach (var a in _allAppointments
                     .Where(x => x.ScheduledAt.Date == _selectedDate.Date)
                     .OrderBy(x => x.ScheduledAt))
        {
            _dayAppointments.Add(a);
        }

        UpdateSummary();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            await LoadDataAsync();
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
        _allAppointments = (await _api.GetDoctorPortalAppointmentsAsync()).ToList();
        ApplyFilter();
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
