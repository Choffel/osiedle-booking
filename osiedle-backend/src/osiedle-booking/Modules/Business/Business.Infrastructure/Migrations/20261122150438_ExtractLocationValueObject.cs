using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtractLocationValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Businesses");

            migrationBuilder.AddColumn<string>(
                name: "location_city",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "location_street",
                table: "Businesses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "location_city",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "location_street",
                table: "Businesses");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Businesses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
