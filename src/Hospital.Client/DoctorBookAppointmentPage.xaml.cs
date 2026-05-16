using Hospital.Client.Services;
using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class DoctorBookAppointmentPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private int _clinicId;

    public DoctorBookAppointmentPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ScheduleDatePicker.MinimumDate = DateTime.Today;
        if (ScheduleDatePicker.Date < ScheduleDatePicker.MinimumDate)
        {
            ScheduleDatePicker.Date = ScheduleDatePicker.MinimumDate;
        }

        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            var me = await _api.GetDoctorPortalMeAsync();
            if (me is null)
            {
                ClinicInfoLabel.Text = "Profil bilgisi alınamadı; poliklinik atanamadı.";
                _clinicId = 0;
                return;
            }

            _clinicId = me.ClinicId;
            ClinicInfoLabel.Text = $"Poliklinik: {me.ClinicName} — Randevular bu birim ve sizin hekim kaydınız ile oluşturulur.";
        });
    }

    private async void OnBookClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        if (_clinicId <= 0)
        {
            ErrorLabel.Text = "Poliklinik bilgisi eksik; lütfen yeniden giriş yapın.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var nid = (NationalIdEntry.Text ?? string.Empty).Trim();
        if (nid.Length != 11 || !nid.All(char.IsDigit))
        {
            ErrorLabel.Text = "Geçerli 11 haneli T.C. kimlik numarası giriniz.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var when = ScheduleDatePicker.Date.Date.Add(ScheduleTimePicker.Time);
        if (when < DateTime.Now.AddMinutes(-5))
        {
            ErrorLabel.Text = "Geçmiş bir saat seçilemez.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var request = new PortalDoctorBookRequest
        {
            PatientNationalId = nid,
            ClinicId = _clinicId,
            ScheduledAt = when,
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

                await DisplayAlert("Randevu", "Hasta için randevu kaydedildi.", "Tamam");
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
