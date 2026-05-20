using Hospital.Client.Services;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class AdminProfilePage : ContentPage
{
    private readonly IAuthTokenStore _auth = AppLocator.Services.GetRequiredService<IAuthTokenStore>();

    public AdminProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var info = new HospitalContactInfo();
        HospitalNameLabel.Text = info.HospitalName;
        DepartmentLabel.Text = info.Department;
        PhoneLabel.Text = $"Santral: {info.Phone}";
        EmergencyLabel.Text = $"Acil hat: {info.EmergencyLine}";
        EmailLabel.Text = info.Email;
        HoursLabel.Text = info.Hours;
        AddressLabel.Text = info.Address;
        var name = string.IsNullOrWhiteSpace(_auth.DisplayName) ? "Yönetici" : _auth.DisplayName;
        SessionLabel.Text = $"Oturum: {name} · {_auth.Role}";
    }
}
