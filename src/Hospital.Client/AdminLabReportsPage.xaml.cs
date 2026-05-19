using System.Collections.ObjectModel;
using System.Text;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class AdminLabReportsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<LabReportListRow> _items = new();
    private int? _filterPatientId;
    private readonly List<LabReportListRow> _rows = new();
    private readonly DebouncedReload _patientFilterDebouncer;

    public AdminLabReportsPage()
    {
        InitializeComponent();
        ReportsCollection.ItemsSource = _items;
        RbViewList.CheckedChanged += OnViewModeChanged;
        RbViewInsight.CheckedChanged += OnViewModeChanged;
        RbViewList.IsChecked = true;
        _patientFilterDebouncer = new DebouncedReload(ApplyPatientFilterFromEntryAsync);
        SearchFilterWiring.WireEntry(PatientIdEntry, _patientFilterDebouncer);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.ToString(), "Tamam");
        }
    }

    private void OnViewModeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value != true)
        {
            return;
        }

        ApplyViewMode();
    }

    private void ApplyViewMode()
    {
        var listMode = RbViewList.IsChecked == true;
        ReportsCollection.IsVisible = listMode;
        InsightScroller.IsVisible = !listMode;
    }

    private void UpdateKpisAndInsight()
    {
        var now = DateTime.Today;
        var from30 = now.AddDays(-30);
        KpiTotal.Text = _rows.Count.ToString();
        KpiCritical.Text = _rows.Count(x => x.IsCriticalAttention).ToString();
        KpiLast30.Text = _rows.Count(x => x.ResultDate >= from30).ToString();

        if (_rows.Count == 0)
        {
            InsightBodyLabel.Text = "İçgörü oluşturmak için kayıt yüklenmelidir.";
            return;
        }

        var sb = new StringBuilder();

        sb.AppendLine("En sık kategoriler:");
        foreach (var g in _rows.GroupBy(r => string.IsNullOrWhiteSpace(r.Category) ? "(Tanımsız)" : r.Category.Trim())
                     .OrderByDescending(g => g.Count())
                     .Take(10))
        {
            sb.AppendLine($"• {g.Key}: {g.Count()} kayıt");
        }

        sb.AppendLine();
        sb.AppendLine("Sonuç tarihi dağılımı (yüklenen küme üzerinden):");
        sb.AppendLine($"• Ortalama sonuç günü: {new DateTime((long)(_rows.Average(r => r.ResultDate.Ticks))):yyyy-MM-dd}");

        InsightBodyLabel.Text = sb.ToString().TrimEnd();
    }

    private async Task ReloadAsync()
    {
        ApplyViewMode();

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetLabReportsAsync(_filterPatientId);
            _rows.Clear();
            _items.Clear();
            foreach (var x in list.OrderByDescending(i => i.ResultDate))
            {
                var row = new LabReportListRow(x);
                _rows.Add(row);
                _items.Add(row);
            }

            UpdateKpisAndInsight();
        });
    }

    private async Task ApplyPatientFilterFromEntryAsync()
    {
        var raw = (PatientIdEntry.Text ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(raw))
        {
            _filterPatientId = null;
            await ReloadAsync();
            return;
        }

        if (!int.TryParse(raw, out var id))
        {
            return;
        }

        _filterPatientId = id;
        await ReloadAsync();
    }

    private async void OnFilterApplyClicked(object? sender, EventArgs e) =>
        await ApplyPatientFilterWithValidationAsync();

    private async Task ApplyPatientFilterWithValidationAsync()
    {
        var raw = (PatientIdEntry.Text ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(raw) && !int.TryParse(raw, out _))
        {
            await DisplayAlert("Filtre", "Geçerli bir hasta ID’si girin veya alanı boş bırakın.", "Tamam");
            return;
        }

        await _patientFilterDebouncer.RunNowAsync();
    }

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

    private async void OnAddClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new LabReportEditPage(null));

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ReportsCollection.SelectedItem is not LabReportListRow r)
        {
            return;
        }

        ReportsCollection.SelectedItem = null;
        await Navigation.PushAsync(new LabReportEditPage(r.Source.Id));
    }
}
