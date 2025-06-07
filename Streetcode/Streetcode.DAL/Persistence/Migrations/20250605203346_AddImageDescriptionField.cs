using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageDescriptionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "terms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "streetcode",
                table: "facts",
                type: "nvarchar(68)",
                maxLength: 68,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ImageDescription",
                schema: "streetcode",
                table: "facts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "terms");

            migrationBuilder.DropColumn(
                name: "ImageDescription",
                schema: "streetcode",
                table: "facts");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "streetcode",
                table: "facts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(68)",
                oldMaxLength: 68);
        }
    }
}
