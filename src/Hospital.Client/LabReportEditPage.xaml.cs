using Hospital.Client.Services;
using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class LabReportEditPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly int? _reportId;
    private readonly List<PatientDto> _patients = new();
    private readonly List<DoctorDto?> _doctors = new();

    public LabReportEditPage(int? reportId)
    {
        InitializeComponent();
        _reportId = reportId;
        DeleteButton.IsVisible = reportId.HasValue;
        if (reportId.HasValue)
        {
            HeroTitle.Text = "Tetkik düzenle";
            PatientPickerSection.IsVisible = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var orderedDoctors = (await _api.GetDoctorsAsync(null, null)).OrderBy(d => d.LastName).ToList();
            _doctors.Clear();
            _doctors.Add(null);
            _doctors.AddRange(orderedDoctors);
            DoctorPicker.Items.Clear();
            DoctorPicker.Items.Add("(Yok)");
            foreach (var d in orderedDoctors)
            {
                DoctorPicker.Items.Add($"{d.FirstName} {d.LastName}");
            }

            if (_reportId is null)
            {
                var patients = await _api.GetPatientsAsync(null, null);
                _patients.Clear();
                _patients.AddRange(patients.OrderBy(p => p.LastName));
                PatientPicker.Items.Clear();
                foreach (var p in _patients)
                {
                    PatientPicker.Items.Add($"{p.FirstName} {p.LastName} — {p.NationalId}");
                }
            }
            else
            {
                var existing = await _api.GetLabReportAsync(_reportId.Value);
                if (existing is null)
                {
                    ErrorLabel.Text = "Kayıt bulunamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                PatientReadOnlyLabel.Text = $"Hasta: {existing.PatientFullName}";
                PatientReadOnlyLabel.IsVisible = true;
                TitleEntry.Text = existing.Title;
                CategoryEntry.Text = existing.Category;
                SummaryEditor.Text = existing.Summary;
                ResultDatePicker.Date = existing.ResultDate.Date;
                if (existing.OrderingDoctorId is { } oid)
                {
                    var idx = orderedDoctors.FindIndex(d => d.Id == oid);
                    DoctorPicker.SelectedIndex = idx >= 0 ? idx + 1 : 0;
                }
                else
                {
                    DoctorPicker.SelectedIndex = 0;
                }
            }

            RefreshCriticalPreview();
        });
    }

    private void OnLabTextChanged(object? sender, TextChangedEventArgs e) =>
        RefreshCriticalPreview();

    private void RefreshCriticalPreview()
    {
        var title = TitleEntry.Text ?? string.Empty;
        var sum = SummaryEditor.Text ?? string.Empty;
        CriticalPreviewPanel.IsVisible = LabPresentation.IsLikelyCritical(sum, title);
    }

    private int? GetSelectedDoctorId()
    {
        if (DoctorPicker.SelectedIndex <= 0)
        {
            return null;
        }

        var d = _doctors[DoctorPicker.SelectedIndex];
        return d?.Id;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                if (_reportId is null)
                {
                    if (PatientPicker.SelectedIndex < 0 || PatientPicker.SelectedIndex >= _patients.Count)
                    {
                        ErrorLabel.Text = "Hasta seçiniz.";
                        ErrorLabel.IsVisible = true;
                        return;
                    }

                    var patient = _patients[PatientPicker.SelectedIndex];
                    var create = new CreateLabReportRequest
                    {
                        PatientId = patient.Id,
                        Title = TitleEntry.Text.Trim(),
                        Category = CategoryEntry.Text.Trim(),
                        Summary = SummaryEditor.Text.Trim(),
                        ResultDate = ResultDatePicker.Date,
                        OrderingDoctorId = GetSelectedDoctorId()
                    };

                    var created = await _api.CreateLabReportAsync(create);
                    if (created is null)
                    {
                        ErrorLabel.Text = "Kayıt oluşturulamadı.";
                        ErrorLabel.IsVisible = true;
                        return;
                    }

                    await DisplayAlert("Kayıt", "Tetkik raporu eklendi.", "Tamam");
                }
                else
                {
                    var existing = await _api.GetLabReportAsync(_reportId.Value);
                    if (existing is null)
                    {
                        ErrorLabel.Text = "Kayıt bulunamadı.";
                        ErrorLabel.IsVisible = true;
                        return;
                    }

                    var update = new UpdateLabReportRequest
                    {
                        PatientId = existing.PatientId,
                        Title = TitleEntry.Text.Trim(),
                        Category = CategoryEntry.Text.Trim(),
                        Summary = SummaryEditor.Text.Trim(),
                        ResultDate = ResultDatePicker.Date,
                        OrderingDoctorId = GetSelectedDoctorId()
                    };

                    await _api.UpdateLabReportAsync(_reportId.Value, update);
                    await DisplayAlert("Kayıt", "Güncellendi.", "Tamam");
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_reportId is null)
        {
            return;
        }

        var ok = await DisplayAlert("Sil", "Bu tetkik kaydını silmek istiyor musunuz?", "Evet", "Hayır");
        if (!ok)
        {
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            await _api.DeleteLabReportAsync(_reportId.Value);
            await Navigation.PopAsync();
        });
    }
}
