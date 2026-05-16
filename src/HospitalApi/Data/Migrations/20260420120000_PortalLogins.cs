using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApi.Data.Migrations;

/// <inheritdoc />
public partial class PortalLogins : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PortalPasswordHash",
            table: "Patients",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PortalPasswordHash",
            table: "Doctors",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PortalUserName",
            table: "Doctors",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Doctors_PortalUserName",
            table: "Doctors",
            column: "PortalUserName",
            unique: true,
            filter: "[PortalUserName] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Doctors_PortalUserName",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "PortalUserName",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "PortalPasswordHash",
            table: "Doctors");

        migrationBuilder.DropColumn(
            name: "PortalPasswordHash",
            table: "Patients");
    }
}
