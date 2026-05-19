using System.Security.Claims;
using System.Text;
using HospitalApi.Data;
using HospitalApi.Endpoints;
using HospitalApi.Entities;
using HospitalApi.Security;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");

if (useSqlite)
{
    var sqlitePath = Path.Combine(AppContext.BaseDirectory, "hospital.db");
    builder.Services.AddDbContext<HospitalDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<HospitalDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddSingleton<IPasswordHasher<Doctor>, PasswordHasher<Doctor>>();
builder.Services.AddSingleton<IPasswordHasher<Patient>, PasswordHasher<Patient>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ILabReportService, LabReportService>();
builder.Services.AddScoped<IDoctorDutyService, DoctorDutyService>();
builder.Services.AddScoped<IPortalAppointmentService, PortalAppointmentService>();
builder.Services.AddScoped<IPortalTimelineService, PortalTimelineService>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
    options.AddPolicy("Doctor", p => p.RequireRole("Doctor"));
    options.AddPolicy("Patient", p => p.RequireRole("Patient"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Mobile", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hastane Yönetim API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Mobile");
app.UseAuthentication();
app.UseAuthorization();

app.MapHospitalEndpoints();
app.MapPortalEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    var appHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    var doctorHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Doctor>>();
    var patientHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Patient>>();
    await DbInitializer.SeedAsync(db, appHasher, doctorHasher, patientHasher);
}

app.Run();
