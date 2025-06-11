using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToStreetcodeCategoryContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.DropColumn(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "terms");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "sources",
                table: "streetcode_source_link_categories",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "streetcode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StreetcodeId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalSchema: "streetcode",
                        principalTable: "comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_streetcodes_StreetcodeId",
                        column: x => x.StreetcodeId,
                        principalSchema: "streetcode",
                        principalTable: "streetcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_streetcode_source_link_categories_SourceLinkCategoryId",
                schema: "sources",
                table: "streetcode_source_link_categories",
                column: "SourceLinkCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_ParentCommentId",
                schema: "streetcode",
                table: "comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_StreetcodeId",
                schema: "streetcode",
                table: "comments",
                column: "StreetcodeId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_UserId",
                schema: "streetcode",
                table: "comments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments",
                schema: "streetcode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.DropIndex(
                name: "IX_streetcode_source_link_categories_SourceLinkCategoryId",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "sources",
                table: "streetcode_source_link_categories");

            migrationBuilder.AddColumn<int>(
                name: "StreetcodeId",
                schema: "streetcode",
                table: "terms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_streetcode_source_link_categories",
                schema: "sources",
                table: "streetcode_source_link_categories",
                columns: new[] { "SourceLinkCategoryId", "StreetcodeId" });
        }
    }
}
