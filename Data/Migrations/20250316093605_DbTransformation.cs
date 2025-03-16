using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class DbTransformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "materials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "materials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImagePath",
                table: "db_metas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DbVersion",
                table: "db_metas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "assignment_items",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "BackgroundImagePath",
                table: "db_metas");

            migrationBuilder.DropColumn(
                name: "DbVersion",
                table: "db_metas");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "assignment_items");
        }
    }
}
