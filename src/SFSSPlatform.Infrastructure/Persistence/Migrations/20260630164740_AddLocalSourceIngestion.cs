using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalSourceIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SourceMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StableKey = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Access = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    PageCount = table.Column<int>(type: "INTEGER", nullable: true),
                    ExtractionStatus = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ExtractionError = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceDocumentChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceMaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    StartPage = table.Column<int>(type: "INTEGER", nullable: false),
                    EndPage = table.Column<int>(type: "INTEGER", nullable: false),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    CharacterCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceDocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceDocumentChunks_SourceMaterials_SourceMaterialId",
                        column: x => x.SourceMaterialId,
                        principalTable: "SourceMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SourceDocumentChunks_SourceMaterialId_Order",
                table: "SourceDocumentChunks",
                columns: new[] { "SourceMaterialId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceDocumentChunks_SourceMaterialId_StartPage",
                table: "SourceDocumentChunks",
                columns: new[] { "SourceMaterialId", "StartPage" });

            migrationBuilder.CreateIndex(
                name: "IX_SourceMaterials_FileName",
                table: "SourceMaterials",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_SourceMaterials_StableKey",
                table: "SourceMaterials",
                column: "StableKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceDocumentChunks");

            migrationBuilder.DropTable(
                name: "SourceMaterials");
        }
    }
}
