using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApi.Data.Migrations;

/// <inheritdoc />
public partial class HealthPortalV1 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "Clinics",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500,
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "LabReports",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PatientId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                ResultDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                OrderingDoctorId = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LabReports", x => x.Id);
                table.ForeignKey(
                    name: "FK_LabReports_Doctors_OrderingDoctorId",
                    column: x => x.OrderingDoctorId,
                    principalTable: "Doctors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_LabReports_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DoctorDuties",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DoctorId = table.Column<int>(type: "int", nullable: false),
                ClinicId = table.Column<int>(type: "int", nullable: false),
                DutyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                DutyKind = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DoctorDuties", x => x.Id);
                table.ForeignKey(
                    name: "FK_DoctorDuties_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DoctorDuties_Doctors_DoctorId",
                    column: x => x.DoctorId,
                    principalTable: "Doctors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabReports_OrderingDoctorId",
            table: "LabReports",
            column: "OrderingDoctorId");

        migrationBuilder.CreateIndex(
            name: "IX_LabReports_PatientId",
            table: "LabReports",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_LabReports_ResultDate",
            table: "LabReports",
            column: "ResultDate");

        migrationBuilder.CreateIndex(
            name: "IX_DoctorDuties_ClinicId",
            table: "DoctorDuties",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_DoctorDuties_DoctorId_DutyDate",
            table: "DoctorDuties",
            columns: new[] { "DoctorId", "DutyDate" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DoctorDuties");

        migrationBuilder.DropTable(
            name: "LabReports");

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "Clinics",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(2000)",
            oldMaxLength: 2000,
            oldNullable: true);
    }
}
