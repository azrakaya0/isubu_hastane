using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApi.Data.Migrations;

/// <inheritdoc />
public partial class ClinicNumber : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            name: "ClinicNumber",
            table: "Clinics");
    }
}
