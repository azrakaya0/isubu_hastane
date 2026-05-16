using HospitalApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Data;

public sealed class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<LabReport> LabReports => Set<LabReport>();
    public DbSet<DoctorDuty> DoctorDuties => Set<DoctorDuty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserName).IsUnique();
            e.Property(x => x.UserName).HasMaxLength(64).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.NationalId).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.NationalId).HasMaxLength(11).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.PortalPasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Clinic>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Specialty).HasMaxLength(200).IsRequired();
            e.Property(x => x.PortalUserName).HasMaxLength(64);
            e.Property(x => x.PortalPasswordHash).HasMaxLength(500);
            e.HasIndex(x => x.PortalUserName).IsUnique().HasFilter("[PortalUserName] IS NOT NULL");
            e.HasOne(x => x.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Clinic)
                .WithMany(c => c.Appointments)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LabReport>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
            e.HasIndex(x => x.PatientId);
            e.HasIndex(x => x.ResultDate);
            e.HasOne(x => x.Patient)
                .WithMany(p => p.LabReports)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.OrderingDoctor)
                .WithMany(d => d.OrderedLabReports)
                .HasForeignKey(x => x.OrderingDoctorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DoctorDuty>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DutyKind).HasMaxLength(80).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => new { x.DoctorId, x.DutyDate });
            e.HasOne(x => x.Doctor)
                .WithMany(d => d.Duties)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Clinic)
                .WithMany(c => c.DoctorDuties)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
