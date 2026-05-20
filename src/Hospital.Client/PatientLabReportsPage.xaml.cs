using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class PatientLabReportsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<LabReportListRow> _items = new();

    public PatientLabReportsPage()
    {
        InitializeComponent();
        LabsCollection.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync() =>
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var list = await _api.GetPatientPortalLabReportsAsync();
            _items.Clear();
            foreach (var x in list.OrderByDescending(i => i.ResultDate))
            {
                _items.Add(new LabReportListRow(x));
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

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LabsCollection.SelectedItem is not LabReportListRow row)
        {
            return;
        }

        LabsCollection.SelectedItem = null;
        await Navigation.PushAsync(new LabReportDetailPage(Clone(row.Source), patientPortal: true));
    }

    private static LabReportDto Clone(LabReportDto r) =>
        new()
        {
            Id = r.Id,
            PatientId = r.PatientId,
            PatientFullName = r.PatientFullName,
            Title = r.Title,
            Category = r.Category,
            Summary = r.Summary,
            ResultDate = r.ResultDate,
            OrderingDoctorId = r.OrderingDoctorId,
            OrderingDoctorName = r.OrderingDoctorName,
            CreatedAt = r.CreatedAt,
            HasPdf = r.HasPdf,
            PdfFileName = r.PdfFileName
        };
}
