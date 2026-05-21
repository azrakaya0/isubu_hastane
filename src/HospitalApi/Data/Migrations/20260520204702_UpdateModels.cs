using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelation",
                table: "Patients",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfFileName",
                table: "LabReports",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeLocation",
                table: "Doctors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicEmail",
                table: "Doctors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicPhone",
                table: "Doctors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicNumber",
                table: "Clinics",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ClinicNumber",
                table: "Clinics",
                column: "ClinicNumber",
                unique: true,
                filter: "[ClinicNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clinics_ClinicNumber",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelation",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PdfFileName",
                table: "LabReports");

            migrationBuilder.DropColumn(
                name: "OfficeLocation",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PublicEmail",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PublicPhone",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ClinicNumber",
                table: "Clinics");
        }
    }
}
