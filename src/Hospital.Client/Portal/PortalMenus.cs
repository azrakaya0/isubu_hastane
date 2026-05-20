namespace Hospital.Client.Portal;

internal static class PortalMenus
{
    internal static IReadOnlyList<PortalMenuItem> AdminItems { get; } =
    [
        new PortalMenuItem("Ana sayfa", "AdminHomePage", () => new AdminHomePage()),
        new PortalMenuItem("Hastalar", "PatientsPage", () => new PatientsPage()),
        new PortalMenuItem("Poliklinikler", "ClinicsPage", () => new ClinicsPage()),
        new PortalMenuItem("Doktorlar", "DoctorsPage", () => new DoctorsPage()),
        new PortalMenuItem("Randevular", "AppointmentsPage", () => new AppointmentsPage()),
        new PortalMenuItem("Tetkik raporları", "AdminLabReportsPage", () => new AdminLabReportsPage()),
        new PortalMenuItem("Nöbet ve izinler", "AdminDoctorDutiesPage", () => new AdminDoctorDutiesPage()),
        new PortalMenuItem("Kurum bilgileri", "AdminProfilePage", () => new AdminProfilePage()),
        new PortalMenuItem("Şifre Değiştir", "ChangePasswordPage", () => new ChangePasswordPage())
    ];

    internal static IReadOnlyList<PortalMenuItem> DoctorItems { get; } =
    [
        new PortalMenuItem("Ana sayfa", "DoctorHomePage", () => new DoctorHomePage()),
        new PortalMenuItem("Takvim", "DoctorCalendarPage", () => new DoctorCalendarPage()),
        new PortalMenuItem("Randevularım", "DoctorAppointmentsPage", () => new DoctorAppointmentsPage()),
        new PortalMenuItem("Hastaya randevu", "DoctorBookAppointmentPage", () => new DoctorBookAppointmentPage()),
        new PortalMenuItem("Geçmiş", "DoctorTimelinePage", () => new DoctorTimelinePage()),
        new PortalMenuItem("Hasta tetkikleri", "DoctorPatientLabsPage", () => new DoctorPatientLabsPage()),
        new PortalMenuItem("Profilim", "DoctorProfilePage", () => new DoctorProfilePage()),
        new PortalMenuItem("Şifre Değiştir", "DoctorChangePasswordPage", () => new ChangePasswordPage())
    ];

    internal static IReadOnlyList<PortalMenuItem> PatientItems { get; } =
    [
        new PortalMenuItem("Ana sayfa", "PatientHomePage", () => new PatientHomePage()),
        new PortalMenuItem("Randevularım", "PatientAppointmentsPage", () => new PatientAppointmentsPage()),
        new PortalMenuItem("Takvim", "PatientAppointmentCalendarPage", () => new PatientAppointmentCalendarPage()),
        new PortalMenuItem("Randevu al", "PatientBookAppointmentPage", () => new PatientBookAppointmentPage()),
        new PortalMenuItem("Sağlık özeti", "PatientTimelinePage", () => new PatientTimelinePage()),
        new PortalMenuItem("Tetkiklerim", "PatientLabReportsPage", () => new PatientLabReportsPage()),
        new PortalMenuItem("Bilgilerim", "PatientProfilePage", () => new PatientProfilePage()),
        new PortalMenuItem("Şifre Değiştir", "PatientChangePasswordPage", () => new ChangePasswordPage())
    ];

    internal static PortalFlyoutHost CreateAdmin() =>
        new(
            windowTitle: "Hastane Yönetimi",
            badge: "HY",
            headline: "Hastane yönetimi",
            tagline: "Hasta · poliklinik · randevu",
            accent: Color.FromArgb("#1565C0"),
            items: AdminItems);

    internal static PortalFlyoutHost CreateDoctor() =>
        new(
            windowTitle: "Doktor portalı",
            badge: "DR",
            headline: "Doktor portalı",
            tagline: "Takvim · randevu · tetkik",
            accent: Color.FromArgb("#1565C0"),
            items: DoctorItems);

    internal static PortalFlyoutHost CreatePatient() =>
        new(
            windowTitle: "Hasta portalı",
            badge: "H",
            headline: "Hasta portalı",
            tagline: "Randevu ve sağlık kayıtları",
            accent: Color.FromArgb("#2E7D32"),
            items: PatientItems);
}
