using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Creator = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    EmbedUrl = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceMaterialId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceDocumentChunkId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsWatched = table.Column<bool>(type: "INTEGER", nullable: false),
                    WatchedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    WatchProgressSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningResources_SourceDocumentChunks_SourceDocumentChunkId",
                        column: x => x.SourceDocumentChunkId,
                        principalTable: "SourceDocumentChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningResources_SourceMaterials_SourceMaterialId",
                        column: x => x.SourceMaterialId,
                        principalTable: "SourceMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_IsWatched",
                table: "LearningResources",
                column: "IsWatched");

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_Provider_ExternalId",
                table: "LearningResources",
                columns: new[] { "Provider", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_SourceDocumentChunkId",
                table: "LearningResources",
                column: "SourceDocumentChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_SourceMaterialId",
                table: "LearningResources",
                column: "SourceMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_Tags",
                table: "LearningResources",
                column: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningResources");
        }
    }
}
