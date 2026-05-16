using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddCriminalRecordEntryToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HardLight: self-reported criminal record (flavour text shown in the
            // Criminal Records console / wanted-list cartridge as Self-reported entries).
            migrationBuilder.AddColumn<string>(
                name: "criminal_record_entry",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "criminal_record_entry",
                table: "profile");
        }
    }
}
