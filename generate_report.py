#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Hastane Yönetim Sistemi – Proje Raporu
26 ister + genel açıklamalar  (ekran görüntüsü YOK, yer tutucu bırakıldı)
"""

import os
from docx import Document
from docx.shared import Pt, RGBColor, Inches, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

PROJ = r"C:\Users\eftal\Desktop\isubu_hastane"
OUT  = os.path.join(PROJ, "proje_raporu.docx")

# ── helpers ──────────────────────────────────────────────────────────────────

def set_font(run, name="Calibri", size=11, bold=False, color=None):
    run.font.name = name
    run.font.size = Pt(size)
    run.font.bold = bold
    if color:
        run.font.color.rgb = RGBColor(*color)

def add_heading(doc, text, level=1):
    p = doc.add_heading(text, level=level)
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    return p

def add_body(doc, text, size=11):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    run = p.add_run(text)
    set_font(run, size=size)
    return p

def add_code(doc, code_text):
    """Gri arka planlı kod bloğu ekler."""
    p = doc.add_paragraph()
    p.paragraph_format.left_indent  = Cm(0.5)
    p.paragraph_format.right_indent = Cm(0.5)
    p.paragraph_format.space_before = Pt(4)
    p.paragraph_format.space_after  = Pt(4)
    pPr = p._p.get_or_add_pPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), 'F0F0F0')
    pPr.append(shd)
    run = p.add_run(code_text.strip())
    run.font.name = "Courier New"
    run.font.size = Pt(9)
    run.font.color.rgb = RGBColor(0x1e, 0x3a, 0x5f)
    return p

def add_placeholder(doc, instruction):
    """Ekran görüntüsü yer tutucusu — italik, kenara çekilmiş mavi kutu."""
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.left_indent  = Cm(1.0)
    p.paragraph_format.right_indent = Cm(1.0)
    p.paragraph_format.space_before = Pt(6)
    p.paragraph_format.space_after  = Pt(6)
    pPr = p._p.get_or_add_pPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), 'E8F4FC')
    pPr.append(shd)
    run = p.add_run(f"( .. {instruction} .. )")
    run.font.name = "Calibri"
    run.font.size = Pt(10)
    run.font.italic = True
    run.font.color.rgb = RGBColor(0x1a, 0x5c, 0x8a)
    return p

def add_note(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Cm(0.8)
    run = p.add_run("Not:  " + text)
    run.font.size = Pt(9)
    run.font.italic = True
    run.font.color.rgb = RGBColor(0x2e, 0x6b, 0xab)

def sep(doc):
    doc.add_paragraph()

def bullet(doc, text, bold_prefix=None):
    p = doc.add_paragraph(style="List Bullet")
    if bold_prefix:
        r = p.add_run(bold_prefix + ": ")
        r.bold = True
        r.font.size = Pt(11)
    p.add_run(text).font.size = Pt(11)
    return p

# ── Belgeyi oluştur ──────────────────────────────────────────────────────────

doc = Document()
for sec in doc.sections:
    sec.left_margin   = Cm(2.5)
    sec.right_margin  = Cm(2.5)
    sec.top_margin    = Cm(2.5)
    sec.bottom_margin = Cm(2.5)

# ════════════════════════════════════════════════════════════════════════════
# KAPAK
# ════════════════════════════════════════════════════════════════════════════
doc.add_paragraph()
doc.add_paragraph()

for txt, size, bold, color in [
    ("HASTANE YÖNETİM SİSTEMİ", 24, True,  (0x1e, 0x3a, 0x5f)),
    ("Hospital Management System", 14, False, (0x55, 0x55, 0x55)),
    ("Programlama II – Dönem Projesi Raporu", 13, True, None),
    ("Teknolojiler: .NET MAUI  ·  ASP.NET Core Minimal API  ·  Entity Framework Core  ·  SQLite / SQL Server  ·  JWT", 10, False, (0x44,0x44,0x44)),
    ("Mayıs 2026", 11, False, None),
]:
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(txt)
    r.font.name = "Calibri"
    r.font.size = Pt(size)
    r.font.bold = bold
    if color:
        r.font.color.rgb = RGBColor(*color)
    doc.add_paragraph()

# ════════════════════════════════════════════════════════════════════════════
# 1. PROJE GENEL TANIMI
# ════════════════════════════════════════════════════════════════════════════
doc.add_page_break()
add_heading(doc, "1. Proje Genel Tanımı", level=1)

add_body(doc, (
    "Bu proje, gerçek bir hastane ortamını dijital olarak yönetmek amacıyla geliştirilmiş "
    "kapsamlı bir Hastane Yönetim Sistemi'dir. Sistemin temel amacı; hasta kayıtlarını tutmak, "
    "poliklinik ve doktor bilgilerini yönetmek, randevu planlaması yapmak, tetkik raporlarını "
    "saklamak ve doktor nöbet/izin takibini gerçekleştirmektir."
))
add_body(doc, (
    "Sistem üç ana yazılım bileşeninden oluşmaktadır:"
))
bullet(doc, ".NET MAUI ile geliştirilmiş masaüstü/mobil istemci uygulaması", "Hospital.Client")
bullet(doc, "ASP.NET Core Minimal API ile geliştirilmiş RESTful backend sunucusu", "HospitalApi")
bullet(doc, "İstemci ve API arasında paylaşılan DTO sınıfları ve enum tanımları", "Hospital.Shared")

add_body(doc, (
    "Kullanıcılar üç farklı role sahiptir: Yönetici (Admin), Doktor ve Hasta. Her rol, "
    "sisteme kendi portalı üzerinden giriş yapar ve yalnızca kendi yetkisi dahilindeki "
    "işlemleri gerçekleştirebilir. Kimlik doğrulama mekanizması JWT (JSON Web Token) tabanlıdır; "
    "token 4 saat geçerlidir. Veritabanı olarak geliştirme ortamında SQLite, üretim ortamında "
    "SQL Server kullanılabilir — bu seçim tek bir yapılandırma bayrağı ile kontrol edilir."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Uygulama açıldığında ilk gelen portal seçim ekranı — 'Yönetim Portalı', 'Doktor Portalı' ve 'Hasta Portalı' seçenekleri görünür şekilde alınacak")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Admin olarak giriş yapıldıktan sonra açılan Ana Sayfa (KPI kartları, istatistikler ve hızlı işlem butonları içeren ekran)")

# ════════════════════════════════════════════════════════════════════════════
# 2. PROJE İSTERLERİ
# ════════════════════════════════════════════════════════════════════════════
doc.add_page_break()
add_heading(doc, "2. Proje İsterleri", level=1)

# ─── İSTER 1 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 1 – Proje .NET MAUI ile Yapılmış mı?", level=2)

add_body(doc, (
    "Proje tamamen .NET MAUI (Multi-platform App UI) teknolojisi kullanılarak geliştirilmiştir. "
    "İstemci uygulaması olan Hospital.Client, MAUI mimarisinin tüm gereksinimlerini karşılar: "
    "XAML tabanlı kullanıcı arayüzü, platform-bağımsız proje yapısı ve MauiApp başlangıç modeli."
))
add_body(doc, (
    "Proje dosyasında <UseMaui>true</UseMaui> tanımı bulunmaktadır; bu ifade, projenin bir "
    ".NET MAUI uygulaması olduğunu derleyiciye ve dağıtım araçlarına bildiren temel direktiftir. "
    "Uygulama MauiProgram.cs dosyası üzerinden başlatılmakta; burada dependency injection "
    "konteynerı, font kaynakları ve servis kayıtları yapılandırılmaktadır. "
    "Hedef framework olarak net10.0-windows10.0.19041.0 tanımlıdır; iOS ve macCatalyst "
    "hedefleri de proje dosyasında listelenmiştir. Sayfalar (Views), ViewModels ve Shell "
    "navigasyon yapısı MAUI mimarisine uygun biçimde düzenlenmiştir."
))

add_heading(doc, "Kod: src/Hospital.Client/Hospital.Client.csproj", level=4)
add_code(doc, """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ApplicationTitle>Hastane Yönetimi</ApplicationTitle>
    <ApplicationId>com.hospitalapp.client</ApplicationId>
    <ApplicationVersion>1</ApplicationVersion>
  </PropertyGroup>
</Project>""")

add_heading(doc, "Kod: src/Hospital.Client/MauiProgram.cs", level=4)
add_code(doc, """public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",    "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf",   "OpenSansSemibold");
            });

        // Servis kayıtları
        builder.Services.AddSingleton<IAuthTokenStore, AuthTokenStore>();
        builder.Services.AddSingleton<IHospitalApiClient, HospitalApiClient>();
        return builder.Build();
    }
}""")

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Çalışan MAUI uygulaması — Windows 11 masaüstünde Admin ana sayfası veya portal seçim ekranı görünür şekilde alınacak")

# ─── İSTER 2 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 2 – Minimal API Projesi Oluşturulmuş mu?", level=2)

add_body(doc, (
    "Backend tarafında HospitalApi adında bir ASP.NET Core Web projesi oluşturulmuştur. "
    "Bu proje, geleneksel Controller/Action yapısını kullanmak yerine ASP.NET Core 7+'da "
    "sunulan Minimal API yaklaşımını benimsemektedir. Minimal API'de her endpoint; "
    "MapGet, MapPost, MapPut veya MapDelete metodu ile doğrudan bir lambda fonksiyonuna "
    "ya da metoda bağlanır. Bu sayede daha az şablon kodu (boilerplate) yazılır ve "
    "her uç noktanın sorumluluğu tek bir yerde toplanır."
))
add_body(doc, (
    "Tüm yönetici uç noktaları HospitalEndpoints.cs dosyasında, hasta portal uç noktaları "
    "ise PortalEndpoints.cs dosyasında tanımlanmıştır. Her iki dosya da "
    "MapHospitalEndpoints() ve MapPortalEndpoints() extension metotları ile Program.cs'e "
    "bağlanmaktadır. Bu yaklaşım, uç noktaları feature bazlı gruplara ayırarak okunabilirliği "
    "artırır. Toplam endpoint sayısı 40'ı aşmaktadır: hasta, klinik, doktor, randevu, "
    "lab raporu, doktor görevi ve kimlik doğrulama işlemleri için ayrı ayrı MapGet/MapPost/"
    "MapPut/MapDelete çağrıları mevcuttur."
))

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs (seçme örnekler)", level=4)
add_code(doc, """public static IEndpointRouteBuilder MapHospitalEndpoints(this IEndpointRouteBuilder app)
{
    // Kimlik doğrulama endpoint'leri
    app.MapPost("/api/auth/login", async (LoginRequest req, IAuthService auth) =>
    {
        var result = await auth.LoginAsync(req);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
    }).AllowAnonymous();

    var api = app.MapGroup("/api").RequireAuthorization("Admin");

    // Hasta CRUD
    api.MapGet("/patients", async (string? search, string? nationalId,
        IPatientService patients) =>
    {
        var list = await patients.GetAsync(
            new PatientListQuery { Search = search, NationalId = nationalId });
        return Results.Ok(list);
    });

    api.MapPost("/patients", async (CreatePatientRequest request,
        IPatientService patients) =>
    {
        var (success, error, patient) = await patients.CreateAsync(request);
        return success
            ? Results.Created($"/api/patients/{patient!.Id}", patient)
            : Results.BadRequest(new { error });
    });

    api.MapPut("/patients/{id:int}", async (int id,
        UpdatePatientRequest request, IPatientService patients) =>
    {
        var (success, error) = await patients.UpdateAsync(id, request);
        return success ? Results.NoContent() : Results.BadRequest(new { error });
    });

    api.MapDelete("/patients/{id:int}", async (int id, IPatientService patients) =>
    {
        var (success, error) = await patients.DeleteAsync(id);
        return success ? Results.NoContent() : Results.BadRequest(new { error });
    });

    return app;
}""")

add_heading(doc, "Kod: src/HospitalApi/Program.cs – endpoint kayıtları", level=4)
add_code(doc, """app.MapHospitalEndpoints();
app.MapPortalEndpoints();""")

add_note(doc, "Swagger UI arayüzü /swagger adresinden erişilebilir olup tüm endpoint'leri listeler ve test etmeye imkân tanır.")

# ─── İSTER 3 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 3 – Entity Framework Core Code First Yaklaşımı", level=2)

add_body(doc, (
    "Veritabanı erişimi Entity Framework Core (EF Core) kütüphanesi ile sağlanmaktadır ve "
    "Code First yaklaşımı benimsenmiştir. Code First yaklaşımında veritabanı şeması; SQL "
    "sorguları yazılarak değil, C# sınıfları (entity'ler) tanımlanarak oluşturulur. "
    "EF Core bu sınıfları inceleyerek gerekli tablo ve sütunları otomatik üretir."
))
add_body(doc, (
    "HospitalDbContext sınıfı, DbContext'ten türer ve her tablo için bir DbSet<T> özelliği "
    "içerir. OnModelCreating metodu içinde Fluent API kullanılarak tablo adları, birincil "
    "anahtar tanımları, zorunlu alan kısıtlamaları (IsRequired), maksimum uzunluklar "
    "(HasMaxLength) ve yabancı anahtar ilişkileri (HasOne/WithMany) tanımlanmaktadır. "
    "Bu yapı sayesinde veritabanı şeması tamamen C# kodu ile kontrol altında tutulur."
))

add_heading(doc, "Kod: src/HospitalApi/Data/HospitalDbContext.cs", level=4)
add_code(doc, """public class HospitalDbContext(DbContextOptions<HospitalDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser>     Users        => Set<AppUser>();
    public DbSet<Patient>     Patients     => Set<Patient>();
    public DbSet<Clinic>      Clinics      => Set<Clinic>();
    public DbSet<Doctor>      Doctors      => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<LabReport>   LabReports   => Set<LabReport>();
    public DbSet<DoctorDuty>  DoctorDuties => Set<DoctorDuty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.NationalId).HasMaxLength(11).IsRequired();
            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Phone).HasMaxLength(20).IsRequired();
            e.HasMany(p => p.Appointments)
             .WithOne(a => a.Patient)
             .HasForeignKey(a => a.PatientId);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.Doctor)
             .WithMany(d => d.Appointments)
             .HasForeignKey(a => a.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}""")

add_heading(doc, "Kod: src/HospitalApi/Entities/Patient.cs", level=4)
add_code(doc, """public class Patient : AuditableEntity
{
    public string  FirstName  { get; set; } = string.Empty;
    public string  LastName   { get; set; } = string.Empty;
    public string  NationalId { get; set; } = string.Empty;
    public string  Phone      { get; set; } = string.Empty;
    public string? Email      { get; set; }
    public DateTime? BirthDate { get; set; }
    public string?  PasswordHash { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<LabReport>   LabReports   { get; set; } = [];
}""")

# ─── İSTER 4 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 4 – Migration'lar Eklenmiş mi?", level=2)

add_body(doc, (
    "EF Core migration sistemi, veritabanı şemasını zaman içinde evrimleştirmek için "
    "kullanılır. Her migration; bir önceki durumdan yeni duruma geçişi tanımlayan Up() "
    "ve geri almayı tanımlayan Down() metotlarını içerir. Böylece takım üyeleri "
    "veritabanını tutan SQL betiklerini elle yazmak zorunda kalmaz; migration komutları "
    "yeterlidir."
))
add_body(doc, (
    "Projede beş adet migration bulunmaktadır. İlk migration (InitialCreate) temel tabloları "
    "oluşturur. Sonraki migration'lar portal giriş alanları, sağlık portalı genişlemesi, "
    "klinik numarası ve model güncellemelerini kapsamaktadır. Uygulama başlangıcında "
    "DbInitializer.SeedAsync() çağrısı, migration'ları uygulayarak demo verileri de "
    "seed eder."
))

add_heading(doc, "Migration Dosyaları (src/HospitalApi/Data/Migrations/)", level=4)
add_code(doc, """20260417132308_InitialCreate.cs        ← Temel tablolar: Users, Patients, Clinics, Doctors,
                                            Appointments, LabReports, DoctorDuties
20260420120000_PortalLogins.cs         ← Doctor ve Patient tablolarına PasswordHash eklendi
20260420190000_HealthPortalV1.cs       ← LabReport ve timeline alanları eklendi
20260519120000_ClinicNumber.cs         ← Clinic tablosuna ClinicNumber alanı eklendi
20260520204702_UpdateModels.cs         ← Randevu slot/durum alanları güncellendi
HospitalDbContextModelSnapshot.cs      ← Güncel şema anlık görüntüsü (otomatik üretilir)""")

add_heading(doc, "Kod: 20260417132308_InitialCreate.cs (kısa özet)", level=4)
add_code(doc, """protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Patients",
        columns: table => new
        {
            Id         = table.Column<int>(nullable: false)
                              .Annotation("SqlServer:Identity", "1, 1"),
            FirstName  = table.Column<string>(maxLength: 100, nullable: false),
            LastName   = table.Column<string>(maxLength: 100, nullable: false),
            NationalId = table.Column<string>(maxLength: 11,  nullable: false),
            Phone      = table.Column<string>(maxLength: 20,  nullable: false),
            CreatedAt  = table.Column<DateTime>(nullable: false),
            // ... diğer sütunlar
        },
        constraints: table => table.PrimaryKey("PK_Patients", x => x.Id));
}""")

add_note(doc, "Migration'lar 'dotnet ef migrations add <Ad>' komutuyla, uygulanmaları ise 'dotnet ef database update' ile yapılır. Program.cs'deki SeedAsync çağrısı bunları otomatik uygular.")

# ─── İSTER 5 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 5 – Veritabanı Bağlantısı Yapılmış mı?", level=2)

add_body(doc, (
    "API projesi hem SQL Server hem de SQLite veritabanı motorlarını desteklemektedir. "
    "Hangi motorun kullanılacağı, appsettings.json dosyasındaki 'UseSqlite' boolean bayrağı "
    "ile belirlenir. Bu yaklaşım; geliştiricilerin Docker veya SQL Server kurulumuna ihtiyaç "
    "duymadan SQLite ile kolayca çalışmasını, üretim ortamında ise SQL Server'a geçişi "
    "yapılandırma değişikliğiyle sağlamasını mümkün kılar."
))
add_body(doc, (
    "SQL Server bağlantısı için connection string, appsettings.Development.json dosyasından "
    "okunur. SQLite modunda ise veritabanı dosyası (hospital.db), uygulamanın çalışma "
    "dizininde (AppContext.BaseDirectory) otomatik olarak oluşturulur. Her iki seçenek "
    "de EF Core'un ilgili DbContextOptions builder metotları ile yapılandırılır."
))

add_heading(doc, "Kod: src/HospitalApi/Program.cs – çift veritabanı desteği", level=4)
add_code(doc, """var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");

if (useSqlite)
{
    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "hospital.db");
    builder.Services.AddDbContext<HospitalDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Bağlantı dizesi bulunamadı.");

    builder.Services.AddDbContext<HospitalDbContext>(options =>
        options.UseSqlServer(connectionString));
}""")

add_heading(doc, "Kod: src/HospitalApi/appsettings.json", level=4)
add_code(doc, """{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HospitalDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "UseSqlite": false,
  "Jwt": {
    "Issuer":        "HospitalApi",
    "Audience":      "HospitalClient",
    "SigningKey":     "ChangeThisDevelopmentKey_32chars_minimum!!",
    "ExpireMinutes":  240
  }
}""")

add_heading(doc, "Kod: src/HospitalApi/Properties/launchSettings.json – SQLite profili", level=4)
add_code(doc, """"http-sqlite": {
  "commandName": "Project",
  "applicationUrl": "http://localhost:5059",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "UseSqlite": "true"
  }
}""")

# ─── İSTER 6 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 6 – Veritabanından Listeleme ve Sorgulama", level=2)

add_body(doc, (
    "Sistemdeki tüm ana varlıklar (hasta, doktor, klinik, randevu, lab raporu, doktor görevi) "
    "veritabanından listelenerek MAUI ekranlarına yansıtılmaktadır. Her listeleme işlemi "
    "filtreleme parametreleri içerir; bu sayede kullanıcılar geniş bir liste içinden "
    "aradıklarını kolayca bulabilir."
))
add_body(doc, (
    "Hasta listesinde ad, soyad, telefon, e-posta veya T.C. kimlik numarasına göre "
    "arama yapılabilir. Randevu listesinde klinik, doktor, tarih aralığı (tümü/bugün/bu hafta) "
    "ve randevu durumuna (Planlandı/Tamamlandı/İptal) göre filtreleme mevcuttur. "
    "Doktor listesi kliniğe göre filtrelenebilir. Tüm bu sorgular sunucu tarafında "
    "EF Core LINQ sorguları ile yapılmakta, gereksiz kayıtlar veritabanı düzeyinde elenerek "
    "ağ trafiği minimize edilmektedir."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Hastalar sayfası — arama kutusu dolu veya filtre uygulanmış hâlde, altında hasta listesi kartları görünür şekilde alınacak")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevular sayfası — klinik filtresi ve tarih RadioButton'ları ile filtreli randevu listesi görünür şekilde alınacak")
sep(doc)

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – LINQ filtreleme zinciri", level=4)
add_code(doc, """public async Task<IReadOnlyList<PatientDto>> GetAsync(
    PatientListQuery query, CancellationToken ct = default)
{
    var linq = db.Patients.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
        var term = query.Search.Trim();
        linq = linq.Where(p =>
            p.FirstName.Contains(term) ||
            p.LastName.Contains(term)  ||
            (p.Email  != null && p.Email.Contains(term)) ||
            p.Phone.Contains(term));
    }

    if (!string.IsNullOrWhiteSpace(query.NationalId))
        linq = linq.Where(p => p.NationalId == query.NationalId.Trim());

    return await linq
        .OrderByDescending(p => p.CreatedAt)
        .Select(p => new PatientDto
        {
            Id         = p.Id,
            FirstName  = p.FirstName,
            LastName   = p.LastName,
            NationalId = p.NationalId,
            Phone      = p.Phone,
            Email      = p.Email,
            BirthDate  = p.BirthDate,
            CreatedAt  = p.CreatedAt
        })
        .ToListAsync(ct);
}""")

add_note(doc, "AsNoTracking() kullanımı: salt okunur sorgularda EF Core'un değişiklik izleme mekanizmasını devre dışı bırakır, performansı artırır.")

# ─── İSTER 7 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 7 – Veritabanına Veri Ekleme", level=2)

add_body(doc, (
    "Sisteme yeni kayıt eklemek için her varlık türüne özel bir POST endpoint'i ve "
    "servis metodu mevcuttur. MAUI istemcisinde her liste sayfasında bir 'Yeni Ekle' "
    "butonu bulunur; bu butona basıldığında ilgili düzenleme sayfası boş açılır. "
    "Kullanıcı formu doldurup 'Kaydet'e bastığında istemci, API'ye POST isteği gönderir."
))
add_body(doc, (
    "Ekleme işleminde önce istek nesnesi (DTO) doğrulanır; zorunlu alanlar boş ise "
    "hata döndürülür. Doğrulama geçilirse entity nesnesi oluşturulur, CreatedAt ve "
    "CreatedByUserId audit alanları doldurulur, veritabanına eklenir ve değişiklikler "
    "kaydedilir. Başarılı ekleme sonucunda HTTP 201 Created yanıtı döner; hata durumunda "
    "HTTP 400 Bad Request ve hata mesajı döner."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Hasta düzenleme/ekleme sayfası (PatientEditPage) — boş formu gösteren ekran: Ad, Soyad, T.C. Kimlik, Telefon, E-posta, Doğum tarihi alanları görünür")
sep(doc)

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs – POST /patients", level=4)
add_code(doc, """api.MapPost("/patients", async (
    CreatePatientRequest request,
    IPatientService patients) =>
{
    var (success, error, patient) = await patients.CreateAsync(request);
    return success
        ? Results.Created($"/api/patients/{patient!.Id}", patient)
        : Results.BadRequest(new { error });
});""")

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – CreateAsync", level=4)
add_code(doc, """public async Task<(bool Success, string? Error, PatientDto? Patient)> CreateAsync(
    CreatePatientRequest request, CancellationToken ct = default)
{
    // T.C. kimlik tekrar kontrolü
    if (await db.Patients.AnyAsync(p => p.NationalId == request.NationalId, ct))
        return (false, "Bu T.C. kimlik numarasıyla kayıtlı hasta zaten mevcut.", null);

    var entity = new Patient
    {
        FirstName       = request.FirstName,
        LastName        = request.LastName,
        NationalId      = request.NationalId,
        Phone           = request.Phone,
        Email           = request.Email,
        BirthDate       = request.BirthDate,
        CreatedAt       = DateTime.UtcNow,
        CreatedByUserId = currentUser.UserId
    };

    if (!string.IsNullOrWhiteSpace(request.Password))
        entity.PasswordHash = patientPasswordHasher.HashPassword(entity, request.Password);

    db.Patients.Add(entity);
    await db.SaveChangesAsync(ct);

    return (true, null, entity.ToDto());
}""")

# ─── İSTER 8 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 8 – Veritabanındaki Veri Üzerinde Güncelleme", level=2)

add_body(doc, (
    "Mevcut kayıtlar güncellenebilir. MAUI istemcisinde listeden bir kayıt seçildiğinde "
    "düzenleme sayfası mevcut verilerle dolu açılır. Kullanıcı istediği alanı değiştirip "
    "'Kaydet'e bastığında istemci, API'ye PUT isteği gönderir."
))
add_body(doc, (
    "Güncelleme işleminde sunucu tarafında güncellenen kayıt önce veritabanından bulunur; "
    "bulunamazsa 404 Not Found döner. Kayıt bulunursa gelen istek verileri entity üzerine "
    "uygulanır, UpdatedAt ve UpdatedByUserId audit alanları güncellenir ve değişiklikler "
    "kaydedilir. Başarılı güncelleme HTTP 204 No Content ile bildirilir."
))

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs – PUT /patients/{id}", level=4)
add_code(doc, """api.MapPut("/patients/{id:int}", async (int id,
    UpdatePatientRequest request, IPatientService patients) =>
{
    var (success, error) = await patients.UpdateAsync(id, request);
    return success
        ? Results.NoContent()
        : Results.BadRequest(new { error });
});""")

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – UpdateAsync", level=4)
add_code(doc, """public async Task<(bool Success, string? Error)> UpdateAsync(
    int id, UpdatePatientRequest request, CancellationToken ct = default)
{
    var entity = await db.Patients.FindAsync([id], ct);
    if (entity is null)
        return (false, "Hasta bulunamadı.");

    entity.FirstName        = request.FirstName;
    entity.LastName         = request.LastName;
    entity.Phone            = request.Phone;
    entity.Email            = request.Email;
    entity.BirthDate        = request.BirthDate;
    entity.UpdatedAt        = DateTime.UtcNow;
    entity.UpdatedByUserId  = currentUser.UserId;

    await db.SaveChangesAsync(ct);
    return (true, null);
}""")

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Hasta düzenleme sayfası mevcut veri dolu hâlde — listeden seçilen hastanın bilgileri formdaki alanlara yüklenmiş görünür")

# ─── İSTER 9 ─────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 9 – Veritabanındaki Veri Silme", level=2)

add_body(doc, (
    "Kayıtlar silinebilir; ancak silme işlemi iş kuralları kontrolleriyle korunmaktadır. "
    "MAUI istemcisinde düzenleme sayfasında veya liste sayfasında 'Sil' butonu bulunur. "
    "Butona basıldığında 'Emin misiniz?' onay diyalogu gösterilir. Onay verildiğinde "
    "istemci API'ye DELETE isteği gönderir."
))
add_body(doc, (
    "Sunucu tarafında referans bütünlüğü kontrol edilir. Örneğin randevusu bulunan bir hasta "
    "silinemez; bu durumda açıklayıcı bir hata mesajı döner. Benzer şekilde doktoru olan "
    "bir klinik veya aktif randevusu olan bir doktor silme işlemine izin vermez. "
    "Silme başarılıysa HTTP 204 No Content döner. MAUI tarafında hata mesajı DisplayAlert "
    "ile kullanıcıya gösterilir."
))

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs – DELETE /patients/{id}", level=4)
add_code(doc, """api.MapDelete("/patients/{id:int}", async (int id, IPatientService patients) =>
{
    var (success, error) = await patients.DeleteAsync(id);
    return success
        ? Results.NoContent()
        : Results.BadRequest(new { error });
});""")

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – DeleteAsync", level=4)
add_code(doc, """public async Task<(bool Success, string? Error)> DeleteAsync(
    int id, CancellationToken ct = default)
{
    var entity = await db.Patients
        .Include(p => p.Appointments)
        .FirstOrDefaultAsync(p => p.Id == id, ct);

    if (entity is null)
        return (false, "Hasta bulunamadı.");

    if (entity.Appointments.Count > 0)
        return (false, "Bu hastanın randevuları var, önce randevuları silin.");

    db.Patients.Remove(entity);
    await db.SaveChangesAsync(ct);
    return (true, null);
}""")

add_heading(doc, "Kod: src/Hospital.Client/PatientsPage.xaml.cs – MAUI silme onay diyalogu", level=4)
add_code(doc, """bool confirmed = await DisplayAlert(
    "Silme Onayı",
    $"{patient.FullName} adlı hastayı silmek istediğinize emin misiniz?",
    "Evet, Sil",
    "İptal");

if (confirmed)
{
    var ok = await _api.DeletePatientAsync(patient.Id);
    if (!ok)
        await DisplayAlert("Hata", "Silme işlemi başarısız.", "Tamam");
}""")

# ─── İSTER 10 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 10 – Ekleme/Güncellemede Kim-Ne Zaman Bilgisi (Audit)", level=2)

add_body(doc, (
    "Tüm ana entity sınıfları (Patient, Doctor, Clinic, Appointment, LabReport, DoctorDuty) "
    "AuditableEntity isimli soyut (abstract) temel sınıftan kalıtım alır. Bu temel sınıf; "
    "her kaydın kim tarafından, ne zaman oluşturulduğunu ve en son kim tarafından, "
    "ne zaman güncellendiğini otomatik olarak takip eden dört alan içerir."
))
add_body(doc, (
    "Kayıt eklenirken CreatedAt ve CreatedByUserId alanları, işlemi gerçekleştiren kullanıcının "
    "kimliği ve o anki UTC zamanıyla doldurulur. Kayıt güncellenirken de UpdatedAt ve "
    "UpdatedByUserId alanları aynı şekilde güncellenir. Bu bilgiler sayesinde bir kaydı kimin "
    "oluşturduğu veya değiştirdiği sonradan sorgulanabilir; denetim (audit) gereklilikleri "
    "sağlanmış olur."
))

add_heading(doc, "Kod: src/HospitalApi/Entities/AuditableEntity.cs", level=4)
add_code(doc, """namespace HospitalApi.Entities;

public abstract class AuditableEntity
{
    public int       Id              { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public int?      CreatedByUserId { get; set; }
    public DateTime? UpdatedAt       { get; set; }
    public int?      UpdatedByUserId { get; set; }
}""")

add_heading(doc, "Kod: PatientService.cs – oluşturma ve güncelleme sırasında audit alanı ataması", level=4)
add_code(doc, """// Oluşturma sırasında (CreateAsync):
var entity = new Patient
{
    // ... diğer alanlar ...
    CreatedAt       = DateTime.UtcNow,
    CreatedByUserId = currentUser.UserId   // JWT token'dan gelen kullanıcı ID'si
};

// Güncelleme sırasında (UpdateAsync):
entity.UpdatedAt        = DateTime.UtcNow;
entity.UpdatedByUserId  = currentUser.UserId;""")

add_note(doc, "ICurrentUserContext servisi, HttpContext üzerinden JWT claim'lerini okuyarak o anki kullanıcının UserId'sini döndürür. Bu sayede audit bilgileri otomatik ve güvenli biçimde doldurulur.")

# ─── İSTER 11 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 11 – Kullanıcı Girişi", level=2)

add_body(doc, (
    "Kullanıcı girişi JWT (JSON Web Token) tabanlı kimlik doğrulama ile sağlanmaktadır. "
    "Sistem üç farklı kullanıcı türünü destekler: Admin, Doktor ve Hasta. Her birinin "
    "giriş ekranı ve API endpoint'i ayrıdır; bu sayede roller birbirinden izole edilmiş olur."
))
add_body(doc, (
    "Uygulama açıldığında portal seçim ekranı gösterilir. Kullanıcı hangi portala "
    "giriş yapmak istediğini seçer (Yönetim / Doktor / Hasta). Seçimden sonra ilgili "
    "giriş ekranına yönlendirilir. Giriş ekranında kullanıcı adı (veya T.C. kimlik) ve "
    "şifre girilir. API'ye POST isteği gönderilir; doğrulama başarılıysa bir JWT token "
    "döner ve istemci bu token'ı güvenli bellek içi depoya (IAuthTokenStore) kaydeder. "
    "Sonraki tüm API isteklerinde bu token Authorization başlığında taşınır."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Yönetici (Admin) giriş ekranı — kullanıcı adı ve şifre alanları, 'Giriş Yap' butonu ve kullanım koşulları CheckBox'ı görünür şekilde alınacak")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Doktor portal giriş ekranı — T.C. kimlik veya doktor kodu ile şifre giriş alanları, kullanım koşulları onay kutusu")
sep(doc)

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs – login endpoint'leri", level=4)
add_code(doc, """// Yönetici (Admin) girişi
app.MapPost("/api/auth/login",
    async (LoginRequest req, IAuthService auth) =>
    {
        var result = await auth.LoginAsync(req);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
    }).AllowAnonymous();

// Doktor portal girişi
app.MapPost("/api/auth/login/doctor",
    async (PortalLoginRequest req, IAuthService auth) =>
    {
        var result = await auth.DoctorLoginAsync(req);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
    }).AllowAnonymous();

// Hasta portal girişi
app.MapPost("/api/auth/login/patient",
    async (PortalLoginRequest req, IAuthService auth) =>
    {
        var result = await auth.PatientLoginAsync(req);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
    }).AllowAnonymous();""")

add_heading(doc, "Kod: src/HospitalApi/Services/AuthService.cs – JWT token üretimi", level=4)
add_code(doc, """var claims = new[]
{
    new Claim(ClaimTypes.Name,           user.Username),
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role,           "Admin")
};

var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer:             jwtOptions.Issuer,
    audience:           jwtOptions.Audience,
    claims:             claims,
    expires:            DateTime.UtcNow.AddMinutes(jwtOptions.ExpireMinutes),
    signingCredentials: creds);

return new LoginResponse
{
    Token    = new JwtSecurityTokenHandler().WriteToken(token),
    Username = user.Username,
    Role     = "Admin"
};""")

# ─── İSTER 12 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 12 – Kullanıcı Çıkışı", level=2)

add_body(doc, (
    "Oturum kapatma işlemi MAUI istemci tarafında gerçekleştirilir. JWT, stateless (durumsuz) "
    "bir kimlik doğrulama mekanizması olduğundan sunucu tarafında bir oturum (session) "
    "kaydı tutulmaz. Çıkış yapmak için token'ın istemci tarafından silinmesi yeterlidir; "
    "token silindiğinde yetkilendirme başlığı gönderilmediği için sonraki API istekleri "
    "401 Unauthorized ile reddedilir."
))
add_body(doc, (
    "Her portal Shell'inin (Admin, Doktor, Hasta) alt menüsünde 'Çıkış Yap' butonu bulunur. "
    "Bu butona basıldığında IAuthTokenStore servisinin Clear() metodu çağrılarak token "
    "bellekten silinir; ardından kullanıcı portal seçim ekranına (LoginNavigationPage) "
    "yönlendirilir. Yönlendirme, Shell navigasyon yığını temizlenerek yapılır; böylece "
    "geri tuşuna basılarak oturuma geri dönülemez."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Sol flyout menüsünün alt kısmı — 'Çıkış Yap' butonunun görüldüğü, versiyon bilgisi ve footer alanı da içeren menü alt bölümü")
sep(doc)

add_heading(doc, "Kod: src/Hospital.Client/AppShell.xaml – FlyoutFooter çıkış butonu", level=4)
add_code(doc, """<Shell.FlyoutFooter>
    <VerticalStackLayout Padding="16,12,16,14"
                         Spacing="10"
                         BackgroundColor="#E8F4FC">
        <views:HospitalFooterBar Margin="0,0,0,4" />
        <Label Text="Oturumu kapatınca tekrar giriş gerekir."
               FontSize="11"
               HorizontalTextAlignment="Center"
               LineBreakMode="WordWrap" />
        <Button Text="Çıkış Yap"
                Style="{StaticResource OutlineButton}"
                Clicked="OnLogoutClicked" />
    </VerticalStackLayout>
</Shell.FlyoutFooter>""")

add_heading(doc, "Kod: src/Hospital.Client/AppShell.xaml.cs – OnLogoutClicked", level=4)
add_code(doc, """private void OnLogoutClicked(object? sender, EventArgs e)
{
    // JWT token'ı bellekten temizle
    var store = AppLocator.Services.GetRequiredService<IAuthTokenStore>();
    store.Clear();

    // Tüm navigasyon yığınını temizleyerek giriş sayfasına dön
    Application.Current!.Windows[0].Page = App.CreateLoginNavigation();
}""")

add_note(doc, "Çıkış işlemi sunucuya bildirilmez çünkü JWT stateless'tır. Token bir kez silindiğinde sunucu onu geçerli kabul etmez (süresi dolmamış olsa dahi istemci onu göndermez).")

# ─── İSTER 13 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 13 – Şifre Değiştirme Sayfası", level=2)

add_body(doc, (
    "Sistemde şifre değiştirme işlemi için ayrı bir sayfa ve API endpoint'i mevcuttur. "
    "Admin panelinde sol flyout menüsünde 'Şifre Değiştir' seçeneği bulunur; "
    "bu seçeneğe tıklandığında ChangePasswordPage açılır."
))
add_body(doc, (
    "Şifre değiştirme formunda üç alan bulunur: mevcut şifre, yeni şifre ve yeni şifre tekrarı. "
    "MAUI tarafında yeni şifre ile tekrarının eşleşip eşleşmediği kontrol edilir; eşleşmezse "
    "kullanıcıya hata gösterilir. İstek API'ye gönderildiğinde sunucu tarafında mevcut "
    "şifre doğrulanır; yanlışsa hata döner. Doğruysa yeni şifre hash'lenerek kaydedilir."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Şifre değiştirme sayfası (ChangePasswordPage) — 'Mevcut şifre', 'Yeni şifre' ve 'Yeni şifre tekrar' alanları ile 'Kaydet' butonu görünür şekilde alınacak")
sep(doc)

add_heading(doc, "Kod: src/HospitalApi/Endpoints/HospitalEndpoints.cs – şifre değiştirme endpoint'i", level=4)
add_code(doc, """app.MapPut("/api/auth/change-password",
    async (ChangePasswordRequest request,
           IAuthService authService,
           ICurrentUserContext currentUser) =>
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return Results.Unauthorized();

        var (success, error) = await authService.ChangePasswordAsync(
            userId.Value, request);

        return success
            ? Results.NoContent()
            : Results.BadRequest(new { error });
    }).RequireAuthorization();""")

add_heading(doc, "Kod: src/HospitalApi/Services/AuthService.cs – ChangePasswordAsync", level=4)
add_code(doc, """public async Task<(bool Success, string? Error)> ChangePasswordAsync(
    int userId, ChangePasswordRequest request)
{
    var user = await db.Users.FindAsync(userId);
    if (user is null) return (false, "Kullanıcı bulunamadı.");

    var verifyResult = hasher.VerifyHashedPassword(
        user, user.PasswordHash, request.CurrentPassword);

    if (verifyResult == PasswordVerificationResult.Failed)
        return (false, "Mevcut şifre yanlış.");

    user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
    await db.SaveChangesAsync();
    return (true, null);
}""")

add_heading(doc, "Kod: AppShell.xaml – Şifre Değiştir menü öğesi", level=4)
add_code(doc, """<FlyoutItem Title="Şifre Değiştir">
    <ShellContent ContentTemplate="{DataTemplate local:ChangePasswordPage}"
                  Route="ChangePasswordPage" />
</FlyoutItem>""")

# ─── İSTER 14 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 14 – CollectionView Kullanımı", level=2)

add_body(doc, (
    "MAUI'nin CollectionView kontrolü, büyük veri kümelerini yüksek performansla ve "
    "özelleştirilebilir şablonlarla listelemek için kullanılır. Geleneksel ListView'a "
    "kıyasla CollectionView; çift yön kaydırma, seçim modu, boş durum görünümü "
    "(EmptyView) ve özel düzen desteği gibi gelişmiş özellikler sunar."
))
add_body(doc, (
    "Projede CollectionView; hasta, doktor, klinik, randevu, lab raporu, doktor görevi "
    "ve hasta zaman çizelgesi gibi tüm liste ekranlarında kullanılmaktadır. Her ekranda "
    "özel bir DataTemplate tanımlanmış, kartlar Border kontrolü ile yuvarlak köşeli tasarım "
    "kazanmıştır. CollectionView, RefreshView içine sarılmıştır; böylece 'aşağı çek-yenile' "
    "(pull-to-refresh) özelliği ücretsiz kazanılmıştır. SelectionMode='Single' ile "
    "tıklama olayı SelectionChanged event'i üzerinden yönetilir."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Hasta listesi sayfası — CollectionView ile oluşturulmuş hasta kartları: her kartta ad soyad, T.C. kimlik ve telefon bilgisi görünür şekilde")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Doktorlar listesi sayfası — CollectionView ile doktor kartları, her birinde fotoğraf alanı, isim, uzmanlık alanı ve bağlı klinik bilgisi")
sep(doc)

add_heading(doc, "Kod: src/Hospital.Client/PatientsPage.xaml – CollectionView + RefreshView", level=4)
add_code(doc, """<RefreshView x:Name="RefreshHost"
             Refreshing="OnRefreshing">
    <CollectionView x:Name="PatientsCollection"
                    SelectionMode="Single"
                    SelectionChanged="OnSelectionChanged">
        <CollectionView.EmptyView>
            <Label Text="Kayıt bulunamadı."
                   HorizontalOptions="Center"
                   FontSize="14"
                   TextColor="{StaticResource TextSecondary}" />
        </CollectionView.EmptyView>
        <CollectionView.ItemTemplate>
            <DataTemplate x:DataType="dto:PatientDto">
                <Border StrokeThickness="1"
                        Margin="12,6"
                        Padding="14,10">
                    <Border.StrokeShape>
                        <RoundRectangle CornerRadius="10" />
                    </Border.StrokeShape>
                    <Grid ColumnDefinitions="*,Auto">
                        <VerticalStackLayout>
                            <Label Text="{Binding FullName}" FontSize="15" FontFamily="OpenSansSemibold" />
                            <Label Text="{Binding NationalId}" FontSize="12" TextColor="{StaticResource TextSecondary}" />
                        </VerticalStackLayout>
                    </Grid>
                </Border>
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</RefreshView>""")

add_heading(doc, "Kod: src/Hospital.Client/PatientsPage.xaml.cs – seçim olayı", level=4)
add_code(doc, """private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
{
    if (e.CurrentSelection.FirstOrDefault() is PatientDto p)
    {
        PatientsCollection.SelectedItem = null;
        await Navigation.PushAsync(new PatientEditPage(p.Id));
    }
}""")

# ─── İSTER 15 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 15 – Picker Kullanımı", level=2)

add_body(doc, (
    "MAUI Picker kontrolü, önceden tanımlı seçenekler listesinden bir öğe seçmek için "
    "kullanılır. Projede Picker; randevu oluştururken klinik ve doktor seçiminde, "
    "randevu ve lab raporu listelerini filtrelerken, hasta zaman çizelgesinde kullanılmaktadır."
))
add_body(doc, (
    "Randevu oluşturma akışında Picker kullanımı özellikle dikkat çekicidir: "
    "Önce klinik Picker'ından bir klinik seçilir; seçim gerçekleştiğinde SelectedIndexChanged "
    "event'i tetiklenir ve seçilen kliniğe ait doktorlar API'den yüklenerek ikinci "
    "Picker'a (DoctorPicker) aktarılır. Böylece doktor listesi, kliniğe bağlı dinamik "
    "olarak güncellenir. Klinik seçilmeden önce DoctorPicker devre dışı (IsEnabled=false) "
    "kalır. Aynı sayfada tarih seçimi için DatePicker, saat seçimi için zaman slot "
    "listesi de kullanılmaktadır."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevu oluşturma veya düzenleme sayfası — 'Klinik seçin' Picker'ı açık veya seçili hâlde, altında 'Doktor seçin' Picker'ı görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevular listesi sayfası — üstte klinik filtresi ve durum filtresi Picker kontrolleri görünür hâlde")
sep(doc)

add_heading(doc, "Kod: src/Hospital.Client/PatientBookAppointmentPage.xaml – Picker çifti", level=4)
add_code(doc, """<!-- Klinik Seçimi -->
<Label Text="Klinik" Style="{StaticResource FieldLabel}" />
<Picker x:Name="ClinicPicker"
        Title="Klinik seçin"
        ItemDisplayBinding="{Binding Name}"
        SelectedIndexChanged="OnClinicChanged" />

<!-- Doktor Seçimi (kliniğe bağlı, dinamik) -->
<Label Text="Doktor" Style="{StaticResource FieldLabel}" />
<Picker x:Name="DoctorPicker"
        Title="Doktor seçin"
        ItemDisplayBinding="{Binding FullName}"
        IsEnabled="{Binding ClinicSelected}" />""")

add_heading(doc, "Kod: src/Hospital.Client/AppointmentsPage.xaml – liste filtresi Picker'ları", level=4)
add_code(doc, """<Picker x:Name="ClinicFilterPicker"
        Title="Tüm klinikler"
        SelectedIndexChanged="OnFilterChanged" />

<Picker x:Name="StatusFilterPicker"
        Title="Tüm durumlar">
    <Picker.Items>
        <x:String>Planlandı</x:String>
        <x:String>Tamamlandı</x:String>
        <x:String>İptal</x:String>
    </Picker.Items>
</Picker>""")

# ─── İSTER 16 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 16 – DatePicker (veya TimePicker) Kullanımı", level=2)

add_body(doc, (
    "MAUI DatePicker kontrolü, kullanıcıdan tarih bilgisi almak için platforma özel "
    "tarih seçici (native date picker) sunar. Bu kontrol; hasta doğum tarihi, randevu tarihi "
    "ve lab raporu tarihlerinin girilmesinde kullanılmaktadır."
))
add_body(doc, (
    "Hasta kayıt/düzenleme formunda (PatientEditPage) bir DatePicker ile hasta doğum tarihi "
    "seçilir. Randevu oluşturma sayfasında ise randevu tarihi DatePicker ile seçildikten "
    "sonra ilgili güne ait müsait zaman slotları API'den yüklenerek liste hâlinde gösterilir. "
    "Lab raporu formunda (LabReportEditPage) da tetkik sonuç tarihi DatePicker ile girilir. "
    "Bu kullanım; ister 16'nın 'DatePicker veya TimePicker kullanılacak' şartını "
    "birden fazla sayfada karşılamaktadır."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Hasta düzenleme sayfasının alt kısmı — 'Doğum tarihi' etiketi ve altındaki DatePicker kontrolü görünür; üst alanlardan bir kısmı da görünür olacak")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevu oluşturma sayfası — tarih seçiminin yapıldığı DatePicker ve altında yüklenen zaman slotları listesi")
sep(doc)

add_heading(doc, "Kod: src/Hospital.Client/PatientEditPage.xaml – doğum tarihi DatePicker", level=4)
add_code(doc, """<Label Text="Doğum tarihi"
       Style="{StaticResource FieldLabel}" />
<DatePicker x:Name="BirthDatePicker"
            Format="d MMMM yyyy"
            HorizontalOptions="Start"
            TextColor="{StaticResource TextPrimary}"
            Date="{x:Static sys:DateTime.Today}" />""")

add_heading(doc, "Kod: src/Hospital.Client/PatientBookAppointmentPage.xaml – randevu tarihi", level=4)
add_code(doc, """<Label Text="Randevu tarihi" Style="{StaticResource FieldLabel}" />
<DatePicker x:Name="ApptDatePicker"
            MinimumDate="{x:Static sys:DateTime.Today}"
            DateSelected="OnDateSelected" />""")

add_heading(doc, "Kod: PatientBookAppointmentPage.xaml.cs – tarih seçiminde slot yükleme", level=4)
add_code(doc, """private async void OnDateSelected(object? sender, DateChangedEventArgs e)
{
    var doctorId = (DoctorPicker.SelectedItem as DoctorDto)?.Id;
    if (doctorId is null) return;

    var slots = await _api.GetAvailableSlotsAsync(doctorId.Value, e.NewDate);
    SlotCollection.ItemsSource = slots;
}""")

# ─── İSTER 17 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 17 – CheckBox ve RadioButton Kullanımı", level=2)

add_body(doc, (
    "Projede hem CheckBox hem de RadioButton kontrolleri aktif olarak kullanılmaktadır. "
    "İster 17, 'CheckBox veya RadioButton' şartını talep etmekte; ancak projede her ikisi de "
    "mevcuttur."
))
add_body(doc, (
    "CheckBox kullanımı: Admin ve doktor giriş ekranlarında 'Kullanım koşullarını okudum ve kabul ediyorum' "
    "onay kutusu (CheckBox) bulunur. Kullanıcı bu onay kutusunu işaretlemeden 'Giriş Yap' "
    "butonu devre dışı kalır (IsEnabled'ı CheckBox'ın IsChecked değerine bağlıdır). "
    "Bu yapı; gizlilik politikası veya kullanım şartlarını kullanıcının açıkça onaylamasını "
    "zorunlu kılmak için gerçekçi bir senaryo oluşturmaktadır."
))
add_body(doc, (
    "RadioButton kullanımı: Randevular sayfasında tarih filtresi için üç adet RadioButton "
    "bulunur: 'Tüm tarihler', 'Bugün' ve 'Bu hafta'. Bu RadioButton'lar aynı GroupName "
    "grubuna ait olduğundan aynı anda yalnızca biri seçili olabilir. Seçim değiştiğinde "
    "CheckedChanged event'i tetiklenerek filtre otomatik uygulanır; kullanıcının ayrıca "
    "'Filtrele' butonuna basması gerekmez."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Admin giriş ekranı — şifre alanının altında 'Kullanım koşullarını kabul ediyorum' CheckBox'ı ve 'Giriş Yap' butonu görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevular sayfasının üst filtre bölümü — 'Tüm tarihler', 'Bugün', 'Bu hafta' RadioButton'larından biri seçili hâlde")
sep(doc)

add_heading(doc, "Kod: src/Hospital.Client/LoginPage.xaml – CheckBox ile koşul kabulü", level=4)
add_code(doc, """<HorizontalStackLayout Spacing="10" Margin="0,8,0,0">
    <CheckBox x:Name="AcceptTermsCheckBox"
              IsCheckedChanged="OnTermsChanged"
              Color="{StaticResource Primary}" />
    <Label Text="Kullanım koşullarını okudum ve kabul ediyorum."
           VerticalOptions="Center"
           FontSize="13" />
</HorizontalStackLayout>

<Button x:Name="LoginButton"
        Text="Giriş Yap"
        IsEnabled="{Binding Source={x:Reference AcceptTermsCheckBox}, Path=IsChecked}"
        Clicked="OnLoginClicked" />""")

add_heading(doc, "Kod: src/Hospital.Client/AppointmentsPage.xaml – RadioButton tarih filtresi", level=4)
add_code(doc, """<HorizontalStackLayout Spacing="16" Margin="12,6,12,0">
    <RadioButton x:Name="RbAll"
                 Content="Tüm tarihler"
                 GroupName="ApptRange"
                 IsChecked="True"
                 CheckedChanged="OnRangeChanged" />
    <RadioButton x:Name="RbToday"
                 Content="Bugün"
                 GroupName="ApptRange"
                 CheckedChanged="OnRangeChanged" />
    <RadioButton x:Name="RbWeek"
                 Content="Bu hafta"
                 GroupName="ApptRange"
                 CheckedChanged="OnRangeChanged" />
</HorizontalStackLayout>""")

# ─── İSTER 18 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 18 – Derste Anlatılmayan MAUI Kontrolleri", level=2)

add_body(doc, (
    "Temel Entry/Label/Button/Image kontrollerinin ötesinde çeşitli ileri düzey MAUI "
    "bileşenleri kullanılmıştır. Bu bileşenler; uygulamanın kullanılabilirliğini "
    "artırmak ve modern bir kullanıcı deneyimi sunmak amacıyla tercih edilmiştir."
))

bullet(doc, "Shell navigasyon mimarisini (FlyoutItem, ShellContent, FlyoutHeader, FlyoutFooter) destekleyen, menü çekmeceli (hamburger) düzeni sağlayan yapı", "Shell")
bullet(doc, "Kullanıcının listeyi aşağı çekerek içeriği yenilemesini sağlayan, CollectionView'ı saran kontrol", "RefreshView")
bullet(doc, "Yuvarlak köşeli kartlar oluşturmak için StrokeShape özelliği ile kullanılan kaplama kontrolü", "Border")
bullet(doc, "Görsellerin dairesel veya yuvarlak köşeli gösteriminde kullanılan şekil sınıfı", "RoundRectangle")
bullet(doc, "İki değer arasında açma/kapama kontrolü (örn: doktor izin aktif/pasif durumu)", "Switch")
bullet(doc, "Fareyle üzerine gelindiğinde imleç tipini değiştiren özel davranış sınıfı", "HandHoverBehavior (Custom Behavior)")
bullet(doc, "Giriş alanlarında klavye tipini ayarlamak için kullanılan, Entry kontrolünün genişletilmiş hali", "Editor (çok satırlı metin)")

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Sol menüsü (flyout) açık hâlde — Shell FlyoutHeader (logo+başlık), menü öğeleri ve FlyoutFooter (Çıkış Yap butonu) tamamı görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Poliklinikler sayfası — Border + RoundRectangle ile oluşturulmuş klinik kartları; yuvarlak köşeli kartlar listesi görünür")
sep(doc)

add_heading(doc, "Kod: AppShell.xaml – Shell + FlyoutHeader + FlyoutFooter", level=4)
add_code(doc, """<Shell FlyoutBehavior="Flyout"
       FlyoutWidth="300"
       Title="Hastane Yönetimi">

    <Shell.FlyoutHeader>
        <views:PortalFlyoutHeader BadgeText="HY"
                                  Headline="Hastane yönetimi"
                                  Tagline="Hasta, poliklinik, doktor ve randevu yönetimi" />
    </Shell.FlyoutHeader>

    <FlyoutItem Title="Ana sayfa">
        <ShellContent ContentTemplate="{DataTemplate local:AdminHomePage}" />
    </FlyoutItem>
    <FlyoutItem Title="Hastalar">
        <ShellContent ContentTemplate="{DataTemplate local:PatientsPage}" />
    </FlyoutItem>
    <!-- diğer menü öğeleri ... -->

    <Shell.FlyoutFooter>
        <VerticalStackLayout BackgroundColor="#E8F4FC" Padding="16,12">
            <Button Text="Çıkış Yap" Clicked="OnLogoutClicked" />
        </VerticalStackLayout>
    </Shell.FlyoutFooter>
</Shell>""")

add_heading(doc, "Kod: PatientsPage.xaml – RefreshView + Border kart", level=4)
add_code(doc, """<RefreshView Refreshing="OnRefreshing">
    <CollectionView SelectionMode="Single" SelectionChanged="OnSelectionChanged">
        <CollectionView.ItemTemplate>
            <DataTemplate>
                <Border StrokeThickness="1"
                        Stroke="{StaticResource CardBorder}"
                        Margin="12,5" Padding="14,12">
                    <Border.StrokeShape>
                        <RoundRectangle CornerRadius="10" />
                    </Border.StrokeShape>
                    <!-- kart içeriği -->
                </Border>
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</RefreshView>""")

# ─── İSTER 19 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 19 – Service Interface'leri", level=2)

add_body(doc, (
    "Proje, Dependency Inversion Principle (DIP) doğrultusunda service interface pattern "
    "kullanmaktadır. Her iş servisi; somut implementasyon sınıfından önce bir interface "
    "ile tanımlanmıştır. API katmanı servis implementasyonlarına doğrudan değil, "
    "interface'leri üzerinden bağımlıdır. Bu sayede testlerde mock implementasyonlar "
    "kolayca enjekte edilebilir ve sınıflar birbirinden gevşek bağımlı (loosely coupled) kalır."
))
add_body(doc, (
    "API tarafında IPatientService, IAuthService, IClinicService, IDoctorService, "
    "IAppointmentService, ILabReportService, IDoctorDutyService, IPortalAppointmentService "
    "ve IPortalTimelineService interface'leri tanımlanmıştır. Client tarafında ise "
    "IHospitalApiClient ve IAuthTokenStore interface'leri mevcuttur."
))

add_heading(doc, "Kod: src/HospitalApi/Services/IPatientService.cs", level=4)
add_code(doc, """namespace HospitalApi.Services;

public interface IPatientService
{
    Task<IReadOnlyList<PatientDto>> GetAsync(
        PatientListQuery query,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, PatientDto? Patient)> CreateAsync(
        CreatePatientRequest request,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> UpdateAsync(
        int id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}""")

add_heading(doc, "Kod: src/HospitalApi/Program.cs – tüm servis interface DI kayıtları", level=4)
add_code(doc, """builder.Services.AddScoped<IAuthService,               AuthService>();
builder.Services.AddScoped<IPatientService,           PatientService>();
builder.Services.AddScoped<IClinicService,            ClinicService>();
builder.Services.AddScoped<IDoctorService,            DoctorService>();
builder.Services.AddScoped<IAppointmentService,       AppointmentService>();
builder.Services.AddScoped<ILabReportService,         LabReportService>();
builder.Services.AddScoped<IDoctorDutyService,        DoctorDutyService>();
builder.Services.AddScoped<IPortalAppointmentService, PortalAppointmentService>();
builder.Services.AddScoped<IPortalTimelineService,    PortalTimelineService>();
builder.Services.AddScoped<ICurrentUserContext,       CurrentUserContext>();""")

# ─── İSTER 20 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 20 – Service Sınıfları", level=2)

add_body(doc, (
    "Her interface'in karşılığında somut bir servis sınıfı bulunmaktadır. Bu sınıflar; "
    "constructor injection ile HospitalDbContext, ICurrentUserContext ve gerekli "
    "yardımcı servisleri alır. Servis sınıflarının tamamı sealed olarak işaretlenmiştir; "
    "bu tasarım kararı, kasıtsız kalıtımın önüne geçer ve derleyicinin optimizasyon "
    "yapmasına yardımcı olur."
))
add_body(doc, (
    "PatientService sınıfı örnek alındığında; GetAsync metodu filtreleme ve sıralama, "
    "CreateAsync metodu yeni hasta kaydı, UpdateAsync metodu güncelleme ve "
    "DeleteAsync metodu silme işlemini gerçekleştirir. Her metot asenkron (async/await) "
    "çalışır ve CancellationToken parametresi ile iptal desteği sağlar."
))

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs (sınıf başlığı ve constructor)", level=4)
add_code(doc, """namespace HospitalApi.Services;

public sealed class PatientService(
    HospitalDbContext           db,
    ICurrentUserContext         currentUser,
    IPasswordHasher<Patient>    patientPasswordHasher) : IPatientService
{
    // GetAsync, CreateAsync, UpdateAsync, DeleteAsync metotları
}""")

add_heading(doc, "Servis Sınıfları Listesi", level=4)
add_code(doc, """AuthService           → IAuthService           (Giriş, token üretimi, şifre değiştirme)
PatientService        → IPatientService        (Hasta CRUD + T.C. kimlik tekrar kontrolü)
ClinicService         → IClinicService         (Klinik CRUD)
DoctorService         → IDoctorService         (Doktor CRUD + şifre hash)
AppointmentService    → IAppointmentService    (Randevu CRUD + slot yönetimi)
LabReportService      → ILabReportService      (Tetkik raporu CRUD + PDF depolama)
DoctorDutyService     → IDoctorDutyService     (Nöbet/izin CRUD)
PortalAppointmentService → IPortalAppointmentService  (Hasta portal randevu akışı)
PortalTimelineService → IPortalTimelineService (Hasta/doktor zaman çizelgesi)
CurrentUserContext    → ICurrentUserContext    (JWT claim'lerinden kullanıcı bilgisi okuma)""")

# ─── İSTER 21 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 21 – OOP (Nesne Yönelimli Programlama) Prensipleri", level=2)

add_body(doc, (
    "Proje, nesne yönelimli programlamanın dört temel prensibini — soyutlama, kapsülleme, "
    "kalıtım ve çok biçimlilik — sistematik biçimde uygular."
))

add_heading(doc, "Soyutlama (Abstraction)", level=4)
add_body(doc, (
    "Interface'ler ve abstract sınıflar sistemi soyutlamak için kullanılır. IPatientService "
    "interface'i, hasta yönetimi için gereken operasyonları tanımlar; ancak nasıl "
    "gerçekleştirildiğini söylemez. AuditableEntity abstract sınıfı, tüm kayıt türleri için "
    "ortak alanları (Id, CreatedAt, vb.) soyut bir sözleşme olarak tanımlar."
))

add_heading(doc, "Kapsülleme (Encapsulation)", level=4)
add_body(doc, (
    "Servis sınıfları sealed olarak işaretlenmiş, iç state private constructor injection "
    "ile saklanmıştır. Entity'lerin alanları property ile erişime açılmıştır. "
    "Şifre hash'leme mantığı servis katmanına kapsüllenmiştir; dışarıdan ham şifre görünmez."
))

add_heading(doc, "Kalıtım (Inheritance)", level=4)
add_body(doc, (
    "Patient, Doctor, Clinic, Appointment, LabReport ve DoctorDuty sınıflarının tamamı "
    "AuditableEntity abstract sınıfından türer. Bu sayede audit alanları her sınıfta tekrar "
    "yazılmaz ve tüm entity'ler tutarlı bir id/zaman/kullanıcı yapısına sahip olur."
))

add_heading(doc, "Çok Biçimlilik (Polymorphism)", level=4)
add_body(doc, (
    "DI konteyneri, interface türüne göre doğru servis sınıfını otomatik enjekte eder. "
    "Endpoint'ler IPatientService interface'i üzerinden çalışır; somut sınıfı bilmeleri "
    "gerekmez. Bu sayede ileride PatientService yerine başka bir implementasyon konulabilir "
    "ve endpoint kodunun tek satırı değişmez."
))

add_heading(doc, "Kod: Kalıtım — entity hiyerarşisi", level=4)
add_code(doc, """// Temel abstract sınıf
public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

// Türetilmiş sınıflar
public class Patient    : AuditableEntity { ... }
public class Doctor     : AuditableEntity { ... }
public class Clinic     : AuditableEntity { ... }
public class Appointment : AuditableEntity { ... }
public class LabReport  : AuditableEntity { ... }
public class DoctorDuty : AuditableEntity { ... }""")

add_heading(doc, "Kod: Çok biçimlilik — DI ile interface → implementasyon eşlemesi", level=4)
add_code(doc, """// DI kaydı (Program.cs)
builder.Services.AddScoped<IPatientService, PatientService>();

// Endpoint'te interface üzerinden kullanım
api.MapGet("/patients", async (IPatientService patients) =>
{
    // 'patients' somut türü PatientService olacak ama kod bunu bilmiyor
    var list = await patients.GetAsync(new PatientListQuery());
    return Results.Ok(list);
});""")

# ─── İSTER 22 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 22 – Form Doğrulama (Validation)", level=2)

add_body(doc, (
    "Veri doğrulama, projede üç ayrı katmanda uygulanmaktadır. Bu yaklaşım; hataların "
    "mümkün olduğunca erken yakalanmasını ve her katmanda anlamlı geri bildirim "
    "sağlanmasını garanti eder."
))

add_body(doc, (
    "Birinci katman — DTO Doğrulama (Data Annotations): "
    "Hospital.Shared projesindeki DTO sınıflarında [Required], [MaxLength], [StringLength], "
    "[Range] ve [EmailAddress] gibi attribute'lar tanımlanmıştır. Bu attribute'lar "
    "Validator.TryValidateObject() metodu aracılığıyla sunucu tarafında kontrol edilir."
))
add_body(doc, (
    "İkinci katman — API Doğrulama (RequestValidator): "
    "HospitalApi'de her POST ve PUT endpoint'i, request nesnesini RequestValidator üzerinden "
    "geçirir. Doğrulama başarısız olursa HTTP 400 Bad Request ve hata detayları döner."
))
add_body(doc, (
    "Üçüncü katman — MAUI İstemci Doğrulama: "
    "MAUI sayfalarında kritik alanlar (zorunlu ad, T.C. kimlik formatı, şifre eşleşmesi vb.) "
    "istek API'ye gönderilmeden önce kod tarafında kontrol edilir. Hata varsa "
    "DisplayAlert diyalogu kullanıcıya gösterilir; API isteği yapılmaz."
))

add_heading(doc, "Kod: src/Hospital.Shared/Dtos/PatientDtos.cs – Data Annotations", level=4)
add_code(doc, """public class CreatePatientRequest
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "T.C. kimlik numarası zorunludur.")]
    [StringLength(11, MinimumLength = 11,
        ErrorMessage = "T.C. kimlik numarası tam 11 haneli olmalıdır.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string? Email { get; set; }
}""")

add_heading(doc, "Kod: src/HospitalApi/Validation/RequestValidator.cs", level=4)
add_code(doc, """public static class RequestValidator
{
    public static IResult? Validate<T>(T instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(instance, context, results, true))
            return null;  // geçerli

        var errors = results
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage!).ToArray());

        return Results.ValidationProblem(errors);
    }
}""")

add_heading(doc, "Kod: src/Hospital.Client/PatientEditPage.xaml.cs – MAUI istemci doğrulama", level=4)
add_code(doc, """private async void OnSaveClicked(object? sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
    {
        await DisplayAlert("Doğrulama Hatası", "Ad alanı boş bırakılamaz.", "Tamam");
        return;
    }
    if (NationalIdEntry.Text?.Length != 11)
    {
        await DisplayAlert("Doğrulama Hatası",
            "T.C. kimlik numarası 11 haneli olmalıdır.", "Tamam");
        return;
    }
    // ... diğer kontroller ...
    await SaveAsync();
}""")

# ─── İSTER 23 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 23 – LINQ Aktif Kullanımı", level=2)

add_body(doc, (
    "LINQ (Language Integrated Query), projede servis katmanındaki tüm veri sorgulama, "
    "filtreleme, sıralama ve dönüştürme işlemlerinde kullanılmaktadır. EF Core, LINQ "
    "sorgularını SQL sorgularına çevirerek doğrudan veritabanında yürütür; bu sayede "
    "gereksiz veri uygulama belleğine aktarılmaz."
))
add_body(doc, (
    "Kullanılan LINQ operatörleri ve amaçları:"
))
bullet(doc, "Filtreleme koşulları zinciri kurmak için", "Where")
bullet(doc, "Entity sınıflarını DTO sınıflarına dönüştürmek için (projeksiyon)", "Select")
bullet(doc, "En yeni kayıtları üste almak için oluşturma tarihine göre ters sıralama", "OrderByDescending")
bullet(doc, "Performans: değişiklik izleyiciyi devre dışı bırakır (read-only sorgular)", "AsNoTracking")
bullet(doc, "Dinamik sorgu zinciri kurmak için (koşula göre Where ekleme)", "AsQueryable")
bullet(doc, "Sonuçları listeye dönüştürür ve SQL sorgusunu çalıştırır", "ToListAsync")
bullet(doc, "Kayıt var mı kontrolü — COUNT(*) yerine EXISTS kullanır", "AnyAsync")
bullet(doc, "İlk eşleşen kayıt veya null döner", "FirstOrDefaultAsync")
bullet(doc, "Navigation property'leri (ilişkili tablolar) eager loading ile çeker", "Include")

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – tam LINQ zinciri", level=4)
add_code(doc, """public async Task<IReadOnlyList<PatientDto>> GetAsync(
    PatientListQuery query, CancellationToken ct = default)
{
    // AsNoTracking: read-only sorgu, EF change tracker kapalı → performans
    var linq = db.Patients.AsNoTracking().AsQueryable();

    // Koşullu filtreleme — her Where sadece kriter girilmişse eklenir
    if (!string.IsNullOrWhiteSpace(query.Search))
    {
        var term = query.Search.Trim();
        linq = linq.Where(p =>
            p.FirstName.Contains(term) ||
            p.LastName.Contains(term)  ||
            (p.Email != null && p.Email.Contains(term)) ||
            p.Phone.Contains(term));
    }
    if (!string.IsNullOrWhiteSpace(query.NationalId))
        linq = linq.Where(p => p.NationalId == query.NationalId.Trim());

    // OrderByDescending: en son eklenen üstte
    // Select: projeksiyon — entity → DTO (sadece gerekli alanlar aktarılır)
    // ToListAsync: sorgu yürütülür, sonuçlar listeye dönüştürülür
    return await linq
        .OrderByDescending(p => p.CreatedAt)
        .Select(p => new PatientDto
        {
            Id         = p.Id,
            FirstName  = p.FirstName,
            LastName   = p.LastName,
            NationalId = p.NationalId,
            Phone      = p.Phone,
            Email      = p.Email,
            BirthDate  = p.BirthDate,
            CreatedAt  = p.CreatedAt
        })
        .ToListAsync(ct);
}""")

add_heading(doc, "Kod: AppointmentService.cs – çoklu tablo sorgusu (Include + Where + Select)", level=4)
add_code(doc, """var linq = db.Appointments
    .AsNoTracking()
    .Include(a => a.Patient)
    .Include(a => a.Doctor)
    .Include(a => a.Clinic)
    .AsQueryable();

if (query.ClinicId.HasValue)
    linq = linq.Where(a => a.ClinicId == query.ClinicId.Value);

if (query.DoctorId.HasValue)
    linq = linq.Where(a => a.DoctorId == query.DoctorId.Value);

if (query.Status.HasValue)
    linq = linq.Where(a => a.Status == query.Status.Value);

return await linq
    .OrderByDescending(a => a.AppointmentDate)
    .Select(a => new AppointmentDto
    {
        Id              = a.Id,
        PatientFullName = a.Patient.FirstName + " " + a.Patient.LastName,
        DoctorFullName  = a.Doctor.FirstName  + " " + a.Doctor.LastName,
        ClinicName      = a.Clinic.Name,
        AppointmentDate = a.AppointmentDate,
        Status          = a.Status
    })
    .ToListAsync(ct);""")

# ─── İSTER 24 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 24 – Proje Çalışıyor mu?", level=2)

add_body(doc, (
    "Proje başarıyla derlenmiş ve çalışır durumdadır. Hospital.sln çözüm dosyası üç projeyi "
    "bir arada yönetir: Hospital.Client (MAUI), HospitalApi (API) ve Hospital.Shared (DTO). "
    "API, SQLite profili ile tek komutla başlatılabilir; MAUI uygulaması Windows 11'de "
    "yerel uygulama olarak çalıştırılmıştır."
))
add_body(doc, (
    "API http://localhost:5059 adresinde hizmet vermekte, MAUI uygulaması bu adrese "
    "HTTP istekleri göndermektedir. Swagger UI /swagger adresinden erişilebilir olup "
    "tüm endpoint'lerin interaktif testine olanak tanır. Veritabanı, ilk çalıştırmada "
    "DbInitializer.SeedAsync() tarafından oluşturulur ve demo verilerle doldurulur: "
    "admin kullanıcısı (admin / Admin123!), örnek doktorlar, klinikler, hastalar ve "
    "randevular otomatik oluşturulur."
))

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: MAUI uygulaması çalışırken Admin ana sayfası — KPI istatistik kartları (toplam hasta, doktor, randevu sayıları) görünür; bu ekran uygulamanın düzgün çalıştığını kanıtlar")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Swagger UI arayüzü — tarayıcıda açık, tüm endpoint grupları (auth, patients, doctors, vb.) listelenir şekilde; isteğe bağlı bu ekran görüntüsü eklenir")
sep(doc)

add_heading(doc, "Build Çıktısı (src/Hospital.Client)", level=4)
add_code(doc, """> dotnet build src/Hospital.Client/Hospital.Client.csproj ^
      --framework net10.0-windows10.0.19041.0

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.35""")

add_heading(doc, "Build Çıktısı (src/HospitalApi)", level=4)
add_code(doc, """> dotnet build src/HospitalApi/HospitalApi.csproj

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.95""")

add_heading(doc, "API Başlatma (SQLite profili)", level=4)
add_code(doc, """> dotnet run --project src/HospitalApi/HospitalApi.csproj ^
      --launch-profile http-sqlite

info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5059
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: HospitalApi.Data.DbInitializer[0]
      Seed tamamlandi: 3 kullanici, 4 klinik, 5 doktor, 10 hasta, 8 randevu.""")

# ─── İSTER 25 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 25 – Hastane Konusuna Uygun Minimum İşlemler", level=2)

add_body(doc, (
    "Proje, gerçek bir hastanede yürütülen temel süreçlerin dijital karşılığını sunmaktadır. "
    "Yalnızca minimum şartları değil; hasta portalı, tetkik raporları, nöbet/izin "
    "yönetimi ve zaman çizelgesi gibi kapsamlı modülleri de barındırmaktadır."
))

add_heading(doc, "Modüller ve Özellikler", level=4)

bullet(doc, "Yeni hasta kaydı oluşturma, mevcut hastayı güncelleme/silme, ad-soyad/TC/telefon/e-posta ile arama ve listeleme", "Hasta Yönetimi")
bullet(doc, "Poliklinik ekleme, güncelleme, silme; klinik numarası ve açıklama yönetimi", "Klinik (Poliklinik) Yönetimi")
bullet(doc, "Doktor kadrosu CRUD; klinik ataması, uzmanlık alanı ve iletişim bilgileri yönetimi", "Doktor Yönetimi")
bullet(doc, "Randevu oluşturma (klinik→doktor→tarih→slot zinciri), listeleme, güncelleme, iptal; durum takibi (Planlandı/Tamamlandı/İptal)", "Randevu Yönetimi")
bullet(doc, "Tetkik sonuç raporu ekleme, görüntüleme ve PDF dosyası yükleme/indirme", "Tetkik (Lab) Raporu Modülü")
bullet(doc, "Doktor nöbet ve izin kayıtlarının oluşturulması, listelenmesi ve düzenlenmesi", "Nöbet / İzin Yönetimi")
bullet(doc, "Hastanın tüm randevularını ve tetkik raporlarını kronolojik zaman çizelgesi olarak görüntülemesi", "Hasta Sağlık Zaman Çizelgesi")
bullet(doc, "Admin / Doktor / Hasta için ayrı shell, giriş ekranı ve yetki alanı; rol bazlı endpoint erişim kontrolü", "Üç Katmanlı Portal (Admin / Doktor / Hasta)")
bullet(doc, "Sistem yöneticisi kullanıcı adı/şifre ile, doktorlar T.C./şifre ile, hastalar T.C./şifre ile sisteme girer; JWT token 4 saat geçerlidir", "Güvenli Kimlik Doğrulama")

sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Sol flyout menüsü açık — Hastalar, Poliklinikler, Doktorlar, Randevular, Tetkik raporları, Nöbet ve izinler menü öğeleri görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Randevular sayfası — randevu listesi ve durum badge'leri (Planlandı/Tamamlandı/İptal renk kodlaması) görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Tetkik (Lab) Raporları sayfası — tetkik listesi ve her raporda görüntüle/indir butonları görünür")
sep(doc)
add_placeholder(doc, "EKRAN GÖRÜNTÜSÜ: Nöbet ve İzinler sayfası — doktor nöbet/izin kayıtları listesi görünür")

# ─── İSTER 26 ────────────────────────────────────────────────────────────────
doc.add_page_break()
add_heading(doc, "İSTER 26 – C# İsimlendirme Kurallarına Uyum", level=2)

add_body(doc, (
    "Projede Microsoft'un resmi C# isimlendirme kuralları (naming conventions) eksiksiz "
    "uygulanmıştır. Bu kurallar; kodun okunabilirliğini artırır, takım içinde tutarlılık "
    "sağlar ve tooling (IDE desteği, analiz araçları) ile uyumlu çalışmayı kolaylaştırır."
))

add_heading(doc, "Uygulanan Kurallar ve Örnekler", level=4)
add_code(doc, """// PascalCase → sınıf, metot, özellik, namespace
public sealed class PatientService { }
public async Task<IReadOnlyList<PatientDto>> GetAsync(...) { }
public string FirstName { get; set; }
namespace HospitalApi.Services;

// I öneki → interface'ler
public interface IPatientService { }
public interface IAuthService { }
public interface ICurrentUserContext { }

// Async soneki → asenkron metotlar
Task GetAsync(...)
Task CreateAsync(...)
Task LoginAsync(...)
Task ChangePasswordAsync(...)

// _camelCase → private instance alanlar (constructor injection'da parametreler PascalCase'dir)
// (C# 12 primary constructor ile doğrudan parametre; field yoktur)
// Ancak geleneksel örnek:
private readonly HospitalDbContext _db;
private bool _loginInProgress;

// camelCase → yerel değişkenler ve metot parametreleri
var entity = new Patient();
string nationalId = request.NationalId;
int userId = currentUser.UserId ?? 0;

// BÜYÜK_HARF → sabit (const) tanımları
public const string AdminRole   = "Admin";
public const string DoctorRole  = "Doctor";
public const string PatientRole = "Patient";""")

add_heading(doc, "Kod: src/HospitalApi/Services/PatientService.cs – kuralların bir arada görüldüğü gerçek örnek", level=4)
add_code(doc, """namespace HospitalApi.Services;          // PascalCase namespace

public sealed class PatientService(      // PascalCase sınıf, sealed
    HospitalDbContext        db,
    ICurrentUserContext      currentUser,  // I öneki interface
    IPasswordHasher<Patient> patientPasswordHasher)
    : IPatientService                      // I öneki interface
{
    public async Task<IReadOnlyList<PatientDto>> GetAsync(   // Async soneki
        PatientListQuery query,
        CancellationToken cancellationToken = default)
    {
        var linq = db.Patients.AsNoTracking().AsQueryable(); // camelCase yerel değişken
        // ...
        return await linq.ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, PatientDto? Patient)> CreateAsync(
        CreatePatientRequest request,              // PascalCase parametre
        CancellationToken ct = default)
    {
        var entity = new Patient                   // camelCase yerel değişken
        {
            FirstName = request.FirstName,         // PascalCase özellik
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };
        db.Patients.Add(entity);
        await db.SaveChangesAsync(ct);
        return (true, null, entity.ToDto());
    }
}""")

add_note(doc, "MAUI client tarafındaki tüm sayfalar (PatientsPage, PatientEditPage, LoginPage vb.), view modeller ve servis sınıfları aynı isimlendirme kurallarına uymaktadır.")

# ════════════════════════════════════════════════════════════════════════════
# 3. MİMARİ ÖZET
# ════════════════════════════════════════════════════════════════════════════
doc.add_page_break()
add_heading(doc, "3. Mimari Özet", level=1)

add_body(doc, (
    "Proje, sorumlulukları net biçimde ayrılmış katmanlı bir mimariye sahiptir. "
    "İstemci ve sunucu arasındaki iletişim HTTP + JWT ile sağlanır; "
    "paylaşılan modeller (DTO'lar) Hospital.Shared projesinde toplanmıştır."
))

add_code(doc, """┌─────────────────────────────────────────────────────────────┐
│            Hospital.Client  (MAUI Masaüstü Uygulaması)      │
│  ┌─────────────┐  ┌─────────────────┐  ┌──────────────────┐ │
│  │  Pages/Views│  │  AppShell.xaml  │  │  MauiProgram.cs  │ │
│  │  (20+ sayfa)│  │  DoctorShell    │  │  DI kayıtları    │ │
│  │             │  │  PatientShell   │  │  Font kaynakları │ │
│  └─────────────┘  └─────────────────┘  └──────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  IHospitalApiClient  ·  IAuthTokenStore                 │ │
│  └─────────────────────────────────────────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │  HTTP/HTTPS  +  JWT Bearer Token
┌────────────────────────▼────────────────────────────────────┐
│              Hospital.Shared  (Paylaşılan Modeller)          │
│  PatientDto, DoctorDto, AppointmentDto, LabReportDto, ...   │
│  LoginRequest, CreatePatientRequest, AppointmentStatus, ... │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│              HospitalApi  (ASP.NET Core Minimal API)         │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  Endpoints:  HospitalEndpoints.cs  /  PortalEndpoints  │ │
│  └───────────────────────┬─────────────────────────────────┘ │
│  ┌────────────────────────▼────────────────────────────────┐ │
│  │  Services:  PatientService, DoctorService, AuthService  │ │
│  │             AppointmentService, LabReportService, ...   │ │
│  └───────────────────────┬─────────────────────────────────┘ │
│  ┌────────────────────────▼────────────────────────────────┐ │
│  │  Entities:  Patient, Doctor, Clinic, Appointment, ...   │ │
│  │  Data:      HospitalDbContext, Migrations/              │ │
│  │  Security:  JWT, PasswordHasher, CurrentUserContext     │ │
│  └───────────────────────┬─────────────────────────────────┘ │
└────────────────────────┬────────────────────────────────────┘
                         │
          ┌──────────────▼──────────────┐
          │  SQLite (geliştirme)        │
          │  SQL Server (üretim)        │
          └─────────────────────────────┘""")

add_body(doc, (
    "Her katmanın tek bir sorumluluğu vardır: MAUI katmanı kullanıcı arayüzünü, "
    "API katmanı iş mantığını, EF Core/veritabanı katmanı ise veri kalıcılığını yönetir. "
    "Katmanlar arasındaki bağımlılıklar interface'ler ve DTO'lar aracılığıyla "
    "gevşek bağımlı (loosely coupled) tutulmuştur."
))

# ════════════════════════════════════════════════════════════════════════════
# 4. SONUÇ
# ════════════════════════════════════════════════════════════════════════════
doc.add_page_break()
add_heading(doc, "4. Sonuç", level=1)

add_body(doc, (
    "Hastane Yönetim Sistemi projesi, belirlenen tüm 26 isteri eksiksiz biçimde karşılamaktadır. "
    "Proje; .NET MAUI ile modern bir masaüstü istemci, ASP.NET Core Minimal API ile sade "
    "ve performanslı bir RESTful backend ve Entity Framework Core Code First ile sağlam "
    "bir veritabanı katmanı olmak üzere üç ana bileşen üzerine inşa edilmiştir."
))
add_body(doc, (
    "Proje boyunca uygulanan önemli teknik kararlar şunlardır: "
    "JWT tabanlı rol bazlı erişim kontrolü (Admin/Doktor/Hasta), "
    "servis interface pattern ile loosely coupled mimari, "
    "AuditableEntity ile otomatik denetim (audit) kaydı, "
    "SQLite/SQL Server çift veritabanı desteği, "
    "CollectionView + RefreshView ile modern liste deneyimi, "
    "Picker/DatePicker/CheckBox/RadioButton/Shell ile zengin MAUI kontrol kullanımı, "
    "üç katmanlı doğrulama (Data Annotations + RequestValidator + MAUI DisplayAlert), "
    "LINQ ile verimli veri sorgulama ve projeksiyon."
))
add_body(doc, (
    "Proje Windows 11 ortamında SQLite veritabanı ile başarıyla çalıştırılmıştır. "
    "Seed data mekanizması sayesinde kurulum gerektirmeden demo kullanıcılar, klinikler, "
    "doktorlar, hastalar ve randevular otomatik oluşturulmaktadır. "
    "Hem API hem MAUI projeleri sıfır hatayla derlenmiş ve işlevsel testleri geçmiştir."
))

# ════════════════════════════════════════════════════════════════════════════
# Kaydet
# ════════════════════════════════════════════════════════════════════════════
doc.save(OUT)
print(f"Rapor olusturuldu: {OUT}")
print("Ekran goruntusu yer tutuculari (mavi italik kutular) kullanici tarafindan doldurulacak.")
