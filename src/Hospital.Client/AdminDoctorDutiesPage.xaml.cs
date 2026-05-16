using System.Collections.ObjectModel;
using System.Text;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class AdminDoctorDutiesPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<DoctorDutyListRow> _items = new();
    private readonly List<DoctorDutyDto> _snapshot = new();

    public AdminDoctorDutiesPage()
    {
        InitializeComponent();
        DutiesCollection.ItemsSource = _items;
        RbViewList.CheckedChanged += OnViewChanged;
        RbViewInsight.CheckedChanged += OnViewChanged;
        OnlyThisMonthCheck.CheckedChanged += (_, _) => ApplyFilter();
        RbViewList.IsChecked = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private void OnViewChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value != true)
        {
            return;
        }

        RefreshViewChrome();
    }

    private void RefreshViewChrome()
    {
        var list = RbViewList.IsChecked == true;
        DutiesCollection.IsVisible = list;
        InsightScroller.IsVisible = !list;
    }

    private static bool LooksLikeShift(string kind) =>
        kind.Contains("nöbet", StringComparison.OrdinalIgnoreCase) ||
        kind.Contains("nobet", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeLeave(string kind) =>
        kind.Contains("izin", StringComparison.OrdinalIgnoreCase) ||
        kind.Contains("rapor", StringComparison.OrdinalIgnoreCase);

    private void UpdateKpis(IReadOnlyList<DoctorDutyListRow> rows)
    {
        KpiTotal.Text = rows.Count.ToString();
        KpiShift.Text = rows.Count(r => LooksLikeShift(r.DutyKind)).ToString();
        KpiLeave.Text = rows.Count(r => LooksLikeLeave(r.DutyKind)).ToString();

        if (rows.Count == 0)
        {
            DutyInsightLabel.Text = "Özet için veri bulunmuyor.";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Görev türüne göre sayım:");
        foreach (var g in rows.GroupBy(r => string.IsNullOrWhiteSpace(r.DutyKind) ? "(Tanımsız)" : r.DutyKind.Trim())
                     .OrderByDescending(g => g.Count()))
        {
            sb.AppendLine($"• {g.Key}: {g.Count()}");
        }

        sb.AppendLine();
        sb.AppendLine("Poliklinik dağılımı (üst 8):");
        foreach (var g in rows.GroupBy(r => r.ClinicName)
                     .OrderByDescending(g => g.Count())
                     .Take(8))
        {
            sb.AppendLine($"• {g.Key}: {g.Count()}");
        }

        DutyInsightLabel.Text = sb.ToString().TrimEnd();
    }

    private void ApplyFilter()
    {
        IEnumerable<DoctorDutyDto> q = _snapshot;
        if (OnlyThisMonthCheck.IsChecked)
        {
            var today = DateTime.Today;
            q = q.Where(d => d.DutyDate.Year == today.Year && d.DutyDate.Month == today.Month);
        }

        var rows = q.OrderByDescending(i => i.DutyDate).ThenBy(i => i.DoctorFullName)
            .Select(d => new DoctorDutyListRow(d))
            .ToList();

        _items.Clear();
        foreach (var r in rows)
        {
            _items.Add(r);
        }

        UpdateKpis(rows);
    }

    private async Task ReloadAsync()
    {
        RefreshViewChrome();

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetDoctorDutiesAsync(null, null, null, null);
            _snapshot.Clear();
            _snapshot.AddRange(list);
            ApplyFilter();
        });
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
        await Navigation.PushAsync(new DoctorDutyEditPage(null));

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DutiesCollection.SelectedItem is not DoctorDutyListRow d)
        {
            return;
        }

        DutiesCollection.SelectedItem = null;
        await Navigation.PushAsync(new DoctorDutyEditPage(d.Source.Id));
    }
}
