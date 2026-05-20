using HospitalApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Data;

public static class DbInitializer
{
    private static readonly (string Name, string Description)[] ExtraClinicSpecs =
    [
        ("Göz",
            "Göz hastalıkları, retina, glokom, katarakt ve refraksiyon bozukluklarında tanı ve tedavi. Görme alanı, OCT ve fundus görüntüleme ile takip; cerrahi öncesi/sonrası danışmanlık bu birim koordinasyonunda yürütülür."),
        ("KBB",
            "İşitme denge ve sinüs hastalıkları, bademcik ve geniz eti değerlendirmesi, ses teli muayeneleri. Endoskopik muayene ve işitme testleri için randevu planlanır; acil KBB durumlarında yönlendirme protokolü uygulanır."),
        ("Nöroloji",
            "Baş ağrısı, baş dönmesi, inme riski, epilepsi ve hafıza şikayetlerinde nörolojik muayene ve tetkik yorumları. EMG/EEG istemleri ve multidisipliner konsültasyonlar poliklinik takvimine göre sunulur."),
        ("Pediatri",
            "0–18 yaş gelişim takibi, aşı programı danışmanlığı, solunum ve sindirim sistemi hastalıkları. Okul çağı çocuklarında büyüme eğrileri ve beslenme önerileri kayıt altına alınır."),
        ("Üroloji",
            "Böbrek taşı, prostat, idrar yolu enfeksiyonları ve minimal invaziv girişim planlaması. İdrar kültürü ve görüntüleme sonuçları hasta dosyasıyla ilişkilendirilir."),
        ("Dermatoloji",
            "Cilt, saç ve tırnak hastalıkları; alerjik deri testleri ve dermoskopik değerlendirme. Kozmetik dermatoloji talepleri tıbbi uygunluk çerçevesinde değerlendirilir."),
        ("Genel Cerrahi",
            "Yumuşak doku, meme, tiroid ve gastrointestinal cerrahi öncesi değerlendirme. Ameliyat öncesi risk analizi ve taburculuk kriterleri hasta güvenliği protokolüne göre yürütülür.")
    ];

    private static readonly (string First, string Last, string Specialty, string ClinicName, string PortalUser)[]
        ExtraDoctorSpecs =
        [
            ("Zeynep", "Çelik", "Göz Hastalıkları", "Göz", "dr.zcelik"),
            ("Emre", "Şahin", "Ortopedi", "Ortopedi", "dr.esahin"),
            ("Pınar", "Koç", "KBB", "KBB", "dr.pkoc"),
            ("Selin", "Arslan", "Nöroloji", "Nöroloji", "dr.sarslan"),
            ("Murat", "En", "Üroloji", "Üroloji", "dr.mentes"),
            ("Defne", "Aydın", "Pediatri", "Pediatri", "dr.dsaydin"),
            ("Kağan", "Yıldız", "Dahiliye", "Dahiliye", "dr.kyildiz"),
            ("Burak", "Tunç", "Kardiyolog", "Kardiyoloji", "dr.btunc"),
            ("Özge", "Özdemir", "Göz Hastalıkları", "Göz", "dr.oozdemir"),
            ("Can", "Güneş", "Ortopedi", "Ortopedi", "dr.cgunes"),
            ("Elif", "Demir", "Dermatoloji", "Dermatoloji", "dr.edemir"),
            ("Hakan", "Vural", "Genel Cerrahi", "Genel Cerrahi", "dr.hvural")
        ];

    public static async Task SeedAsync(
        HospitalDbContext db,
        IPasswordHasher<AppUser> passwordHasher,
        IPasswordHasher<Doctor> doctorPasswordHasher,
        IPasswordHasher<Patient> patientPasswordHasher,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.IsSqlite())
            await db.Database.EnsureCreatedAsync(cancellationToken);
        else
            await db.Database.MigrateAsync(cancellationToken);

        await EnsureClinicNumberSchemaAsync(db, cancellationToken);
        await EnsureExtendedProfileSchemaAsync(db, cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            await EnsureDemoPortalCredentialsAsync(db, doctorPasswordHasher, patientPasswordHasher, cancellationToken);
            await EnsureClinicAndDoctorScaleAsync(db, doctorPasswordHasher, patientPasswordHasher, cancellationToken);
            await EnsureClinicNumbersAsync(db, cancellationToken);
            await EnsureLabsAndDutiesAsync(db, cancellationToken);
            return;
        }

        var admin = new AppUser
        {
            UserName = "admin",
            FullName = "Sistem Yöneticisi",
            Role = "Admin"
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");
        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        var clinics = new List<Clinic>
        {
            new()
            {
                Name = "Dahiliye",
                Description =
                    "Dahiliye polikliniği; hipertansiyon, diyabet, tiroid, solunum ve böbrek hastalıklarında tanı-takip sunar. Kronik hastalık protokollerinde ilaç düzenlemeleri ve tetkik planlaması elektronik hasta dosyası üzerinden izlenir. Endokrin, gastroenteroloji ve nefroloji konsültasyonları ile entegre çalışır.",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            },
            new()
            {
                Name = "Kardiyoloji",
                Description =
                    "Kalp ve damar hastalıklarında risk değerlendirmesi, EKO, efor testi ve Holter istemi. Koroner arter hastalığı, aritmi ve kalp yetmezliğinde tedavi planı oluşturulur; ameliyat öncesi kardiyolojik uygunluk raporları hazırlanır.",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            },
            new()
            {
                Name = "Ortopedi",
                Description =
                    "Travma sonrası kırık-b çıkık, artroz, spor yaralanmaları ve omurga şikayetlerinde ortopedik muayene. Görüntüleme sonuçları (MR, BT) ile cerrahi/girişimsiz seçenekler hasta ile paylaşılır; fizik tedavi yönlendirmesi yapılır.",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            }
        };
        foreach (var (name, desc) in ExtraClinicSpecs)
        {
            clinics.Add(new Clinic
            {
                Name = name,
                Description = desc,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            });
        }

        for (var i = 0; i < clinics.Count; i++)
        {
            clinics[i].ClinicNumber = (101 + i).ToString();
        }

        db.Clinics.AddRange(clinics);
        await db.SaveChangesAsync(cancellationToken);

        Clinic ClinicByName(string name) => clinics.First(c => c.Name == name);

        var doctorSeeds = new List<(string Fn, string Ln, string Spec, string Clinic, string Portal)>
        {
            ("Ayşe", "Yılmaz", "Dahiliye", "Dahiliye", "dr.ayilmaz"),
            ("Mehmet", "Kaya", "Kardiyolog", "Kardiyoloji", "dr.mkaya")
        };
        doctorSeeds.AddRange(ExtraDoctorSpecs.Select(x => (x.First, x.Last, x.Specialty, x.ClinicName, x.PortalUser)));

        foreach (var (fn, ln, spec, clinicName, portal) in doctorSeeds)
        {
            var clinic = ClinicByName(clinicName);
            var doctor = new Doctor
            {
                FirstName = fn,
                LastName = ln,
                Specialty = spec,
                ClinicId = clinic.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id,
                PortalUserName = portal
            };
            doctor.PortalPasswordHash = doctorPasswordHasher.HashPassword(doctor, "Doctor123!");
            db.Doctors.Add(doctor);
        }

        await db.SaveChangesAsync(cancellationToken);

        var demoPatient = new Patient
        {
            FirstName = "Ali",
            LastName = "Veli",
            NationalId = "12345678901",
            Phone = "05550001122",
            BirthDate = new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = admin.Id
        };
        demoPatient.PortalPasswordHash = patientPasswordHasher.HashPassword(demoPatient, "Patient123!");
        db.Patients.Add(demoPatient);

        var eftalyaPatient = new Patient
        {
            FirstName = "Eftalya Beril",
            LastName = "Şahin",
            NationalId = "11173221086",
            Phone = "05551112233",
            BirthDate = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = admin.Id
        };
        eftalyaPatient.PortalPasswordHash = patientPasswordHasher.HashPassword(eftalyaPatient, "Patient123!");
        db.Patients.Add(eftalyaPatient);

        await db.SaveChangesAsync(cancellationToken);

        await SeedLabsAndDutiesForDemoAsync(db, admin.Id, cancellationToken);
    }

    private static async Task SeedLabsAndDutiesForDemoAsync(HospitalDbContext db, int adminId, CancellationToken cancellationToken)
    {
        var demoPatient = await db.Patients.FirstAsync(p => p.NationalId == "12345678901", cancellationToken);
        var ayse = await db.Doctors.FirstAsync(d => d.PortalUserName == "dr.ayilmaz", cancellationToken);

        if (!await db.LabReports.AnyAsync(l => l.PatientId == demoPatient.Id, cancellationToken))
        {
            db.LabReports.AddRange(
                new LabReport
                {
                    PatientId = demoPatient.Id,
                    Title = "Tam kan sayımı",
                    Category = "Kan",
                    Summary =
                        "Hemoglobin 14.2 g/dL, lökosit 7.1 K/µL, trombosit 248 K/µL. Genel tablo fizyolojik sınırlarda; klinik korelasyon önerilir.",
                    ResultDate = DateTime.UtcNow.AddDays(-5),
                    OrderingDoctorId = ayse.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminId
                },
                new LabReport
                {
                    PatientId = demoPatient.Id,
                    Title = "Tiroid fonksiyonları",
                    Category = "Kan",
                    Summary = "TSH 1.8 mIU/L, sT4 1.1 ng/dL. Biyokimyasal olarak ötiroid tablo.",
                    ResultDate = DateTime.UtcNow.AddDays(-12),
                    OrderingDoctorId = ayse.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminId
                });
        }

        if (!await db.DoctorDuties.AnyAsync(d => d.DoctorId == ayse.Id, cancellationToken))
        {
            db.DoctorDuties.AddRange(
                new DoctorDuty
                {
                    DoctorId = ayse.Id,
                    ClinicId = ayse.ClinicId,
                    DutyDate = DateTime.UtcNow.Date,
                    DutyKind = "Poliklinik mesaisi",
                    Notes = "09:00–17:00 poliklinik",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminId
                },
                new DoctorDuty
                {
                    DoctorId = ayse.Id,
                    ClinicId = ayse.ClinicId,
                    DutyDate = DateTime.UtcNow.Date.AddDays(3),
                    DutyKind = "Nöbet",
                    Notes = "Akşam nöbeti — acil konsültasyon",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminId
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureLabsAndDutiesAsync(HospitalDbContext db, CancellationToken cancellationToken)
    {
        var admin = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(cancellationToken);
        if (admin is null)
        {
            return;
        }

        var demoPatient = await db.Patients.FirstOrDefaultAsync(p => p.NationalId == "12345678901", cancellationToken);
        var ayse = await db.Doctors.FirstOrDefaultAsync(d => d.PortalUserName == "dr.ayilmaz", cancellationToken);
        if (demoPatient is null || ayse is null)
        {
            return;
        }

        var changed = false;
        if (!await db.LabReports.AnyAsync(l => l.PatientId == demoPatient.Id, cancellationToken))
        {
            db.LabReports.Add(new LabReport
            {
                PatientId = demoPatient.Id,
                Title = "Tam kan sayımı",
                Category = "Kan",
                Summary = "Örnek sonuçlar portalda görüntülenir; detay için poliklinik başvurusu önerilir.",
                ResultDate = DateTime.UtcNow.AddDays(-3),
                OrderingDoctorId = ayse.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            });
            changed = true;
        }

        if (!await db.DoctorDuties.AnyAsync(d => d.DoctorId == ayse.Id, cancellationToken))
        {
            db.DoctorDuties.Add(new DoctorDuty
            {
                DoctorId = ayse.Id,
                ClinicId = ayse.ClinicId,
                DutyDate = DateTime.UtcNow.Date,
                DutyKind = "Poliklinik mesaisi",
                Notes = "Örnek nöbet kaydı",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureDemoPortalCredentialsAsync(
        HospitalDbContext db,
        IPasswordHasher<Doctor> doctorPasswordHasher,
        IPasswordHasher<Patient> patientPasswordHasher,
        CancellationToken cancellationToken)
    {
        var changed = false;

        var ayse = await db.Doctors.FirstOrDefaultAsync(
            d => d.FirstName == "Ayşe" && d.LastName == "Yılmaz",
            cancellationToken);
        if (ayse is not null && string.IsNullOrEmpty(ayse.PortalUserName))
        {
            ayse.PortalUserName = "dr.ayilmaz";
            ayse.PortalPasswordHash = doctorPasswordHasher.HashPassword(ayse, "Doctor123!");
            changed = true;
        }

        var mehmet = await db.Doctors.FirstOrDefaultAsync(
            d => d.FirstName == "Mehmet" && d.LastName == "Kaya",
            cancellationToken);
        if (mehmet is not null && string.IsNullOrEmpty(mehmet.PortalUserName))
        {
            mehmet.PortalUserName = "dr.mkaya";
            mehmet.PortalPasswordHash = doctorPasswordHasher.HashPassword(mehmet, "Doctor123!");
            changed = true;
        }

        var demoPatient = await db.Patients.FirstOrDefaultAsync(
            p => p.NationalId == "12345678901",
            cancellationToken);
        if (demoPatient is null)
        {
            var admin = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(cancellationToken);
            if (admin is not null)
            {
                demoPatient = new Patient
                {
                    FirstName = "Ali",
                    LastName = "Veli",
                    NationalId = "12345678901",
                    Phone = "05550001122",
                    BirthDate = new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = admin.Id
                };
                demoPatient.PortalPasswordHash = patientPasswordHasher.HashPassword(demoPatient, "Patient123!");
                db.Patients.Add(demoPatient);
                changed = true;
            }
        }
        else if (string.IsNullOrEmpty(demoPatient.PortalPasswordHash))
        {
            demoPatient.PortalPasswordHash = patientPasswordHasher.HashPassword(demoPatient, "Patient123!");
            changed = true;
        }

        var eftalya = await db.Patients.FirstOrDefaultAsync(
            p => p.NationalId == "11173221086",
            cancellationToken);
        if (eftalya is null)
        {
            var adminUser = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(cancellationToken);
            if (adminUser is not null)
            {
                eftalya = new Patient
                {
                    FirstName = "Eftalya Beril",
                    LastName = "Şahin",
                    NationalId = "11173221086",
                    Phone = "05551112233",
                    BirthDate = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                };
                eftalya.PortalPasswordHash = patientPasswordHasher.HashPassword(eftalya, "Patient123!");
                db.Patients.Add(eftalya);
                changed = true;
            }
        }
        else if (string.IsNullOrEmpty(eftalya.PortalPasswordHash))
        {
            eftalya.PortalPasswordHash = patientPasswordHasher.HashPassword(eftalya, "Patient123!");
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureClinicAndDoctorScaleAsync(
        HospitalDbContext db,
        IPasswordHasher<Doctor> doctorPasswordHasher,
        IPasswordHasher<Patient> patientPasswordHasher,
        CancellationToken cancellationToken)
    {
        var admin = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(cancellationToken);
        if (admin is null)
        {
            return;
        }

        var changed = false;

        foreach (var (name, desc) in ExtraClinicSpecs)
        {
            if (await db.Clinics.AnyAsync(c => c.Name == name, cancellationToken))
            {
                continue;
            }

            db.Clinics.Add(new Clinic
            {
                Name = name,
                Description = desc,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id
            });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
            await EnsureClinicNumbersAsync(db, cancellationToken);
        }

        changed = false;
        foreach (var (first, last, specialty, clinicName, portalUser) in ExtraDoctorSpecs)
        {
            if (await db.Doctors.AnyAsync(d => d.PortalUserName == portalUser, cancellationToken))
            {
                continue;
            }

            var clinic = await db.Clinics.FirstOrDefaultAsync(c => c.Name == clinicName, cancellationToken);
            if (clinic is null)
            {
                continue;
            }

            var doctor = new Doctor
            {
                FirstName = first,
                LastName = last,
                Specialty = specialty,
                ClinicId = clinic.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = admin.Id,
                PortalUserName = portalUser
            };
            doctor.PortalPasswordHash = doctorPasswordHasher.HashPassword(doctor, "Doctor123!");
            db.Doctors.Add(doctor);
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureExtendedProfileSchemaAsync(HospitalDbContext db, CancellationToken cancellationToken)
    {
        if (!db.Database.IsSqlite())
        {
            return;
        }

        foreach (var sql in new[]
                 {
                     "ALTER TABLE Patients ADD COLUMN EmergencyContactName TEXT NULL;",
                     "ALTER TABLE Patients ADD COLUMN EmergencyContactPhone TEXT NULL;",
                     "ALTER TABLE Patients ADD COLUMN EmergencyContactRelation TEXT NULL;",
                     "ALTER TABLE Doctors ADD COLUMN OfficeLocation TEXT NULL;",
                     "ALTER TABLE Doctors ADD COLUMN PublicPhone TEXT NULL;",
                     "ALTER TABLE Doctors ADD COLUMN PublicEmail TEXT NULL;",
                     "ALTER TABLE LabReports ADD COLUMN PdfFileName TEXT NULL;"
                 })
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch
            {
                // Sütun zaten var.
            }
        }
    }

    private static async Task EnsureClinicNumberSchemaAsync(HospitalDbContext db, CancellationToken cancellationToken)
    {
        if (!db.Database.IsSqlite())
        {
            return;
        }

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Clinics ADD COLUMN ClinicNumber TEXT NULL;",
                cancellationToken);
        }
        catch
        {
            // Sütun zaten var.
        }
    }

    private static async Task EnsureClinicNumbersAsync(HospitalDbContext db, CancellationToken cancellationToken)
    {
        var clinics = await db.Clinics.OrderBy(c => c.Id).ToListAsync(cancellationToken);
        var next = 101;
        var changed = false;
        foreach (var clinic in clinics)
        {
            if (!string.IsNullOrWhiteSpace(clinic.ClinicNumber))
            {
                if (int.TryParse(clinic.ClinicNumber, out var n) && n >= next)
                {
                    next = n + 1;
                }

                continue;
            }

            while (clinics.Any(c => c.Id != clinic.Id && c.ClinicNumber == next.ToString()))
            {
                next++;
            }

            clinic.ClinicNumber = next.ToString();
            next++;
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
