using Hospital.Client.Services;
using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class DoctorBookAppointmentPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private int _clinicId;
    private int _doctorId;
    private AppointmentSlotPickerHelper? _slotHelper;

    public DoctorBookAppointmentPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ScheduleDatePicker.MinimumDate = DateTime.Today;
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var me = await _api.GetDoctorPortalMeAsync();
            if (me is null)
            {
                ClinicInfoLabel.Text = "Profil bilgisi alınamadı.";
                return;
            }

            _clinicId = me.ClinicId;
            _doctorId = me.Id;
            ClinicInfoLabel.Text =
                $"Poliklinik: {me.ClinicName} — Randevu kaydedildiğinde Randevularım ve Takvim'de görünür.";

            _slotHelper = new AppointmentSlotPickerHelper(
                ScheduleDatePicker,
                SlotPicker,
                () => Task.FromResult<int?>(_doctorId),
                (_, date, __) => _api.GetDoctorPortalAppointmentSlotsAsync(date),
                SlotHintLabel);
            await _slotHelper.InitializeAsync();
        });
    }

    private async void OnBookClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (_clinicId <= 0 || _doctorId <= 0)
        {
            ErrorLabel.Text = "Oturum bilgisi eksik; yeniden giriş yapın.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var when = _slotHelper?.GetSelectedDateTime();
        if (when is null)
        {
            ErrorLabel.Text = "Uygun bir saat seçiniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var nid = (NationalIdEntry.Text ?? string.Empty).Trim();
        var request = new PortalDoctorBookRequest
        {
            PatientNationalId = string.IsNullOrEmpty(nid) ? null : nid,
            WalkInPatient = new AppointmentWalkInPatientRequest
            {
                NationalId = string.IsNullOrEmpty(nid) ? null : nid,
                FirstName = FirstNameEntry.Text?.Trim() ?? string.Empty,
                LastName = LastNameEntry.Text?.Trim() ?? string.Empty,
                Phone = PhoneEntry.Text?.Trim()
            },
            ClinicId = _clinicId,
            ScheduledAt = when.Value,
            Status = AppointmentStatus.Scheduled,
            Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim()
        };

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var created = await _api.DoctorPortalBookAppointmentAsync(request);
                if (created is null)
                {
                    ErrorLabel.Text = "Randevu oluşturulamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                await DisplayAlert(
                    "Randevu",
                    $"Kayıt oluşturuldu.\n{created.PatientFullName}\n{created.ScheduledAt:dd.MM.yyyy HH:mm}\nRandevularım ve Takvim'de listelenir.",
                    "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }
}
