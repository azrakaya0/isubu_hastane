using System.Collections.ObjectModel;
using Hospital.Client.Models;
using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class DoctorPatientLabsPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly ObservableCollection<LabReportListRow> _items = new();
    private int? _patientId;

    public DoctorPatientLabsPage()
    {
        InitializeComponent();
        LabsCollection.ItemsSource = _items;
    }

    private async void OnLookupClicked(object? sender, EventArgs e)
    {
        LookupErrorLabel.IsVisible = false;
        PatientFoundLabel.IsVisible = false;
        _patientId = null;
        _items.Clear();

        var nid = (NationalIdEntry.Text ?? string.Empty).Trim();
        if (nid.Length != 11 || !nid.All(char.IsDigit))
        {
            LookupErrorLabel.Text = "11 haneli T.C. kimlik numarası giriniz.";
            LookupErrorLabel.IsVisible = true;
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var patient = await _api.DoctorPortalLookupPatientAsync(nid);
                if (patient is null)
                {
                    LookupErrorLabel.Text = "Hasta bulunamadı.";
                    LookupErrorLabel.IsVisible = true;
                    return;
                }

                _patientId = patient.Id;
                PatientFoundLabel.Text = $"{patient.FirstName} {patient.LastName}";
                PatientFoundLabel.IsVisible = true;
                await LoadLabsAsync();
            }
            catch (Exception ex)
            {
                LookupErrorLabel.Text = ex.Message;
                LookupErrorLabel.IsVisible = true;
            }
        });
    }

    private async Task LoadLabsAsync()
    {
        if (_patientId is null)
        {
            return;
        }

        var list = await _api.GetDoctorPortalPatientLabsAsync(_patientId.Value);
        _items.Clear();
        foreach (var x in list.OrderByDescending(i => i.ResultDate))
        {
            _items.Add(new LabReportListRow(x));
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        try
        {
            if (_patientId is not null)
            {
                await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, LoadLabsAsync);
            }
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
        await Navigation.PushAsync(new LabReportDetailPage(Clone(row.Source)));
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
            CreatedAt = r.CreatedAt
        };
}
