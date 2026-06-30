using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudySessionAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "StudyItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConfidenceScore",
                table: "StudyItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptAt",
                table: "StudyItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextReviewAt",
                table: "StudyItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudyAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudyItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyAttempts_StudyItems_StudyItemId",
                        column: x => x.StudyItemId,
                        principalTable: "StudyItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudyItems_Status_NextReviewAt_Id",
                table: "StudyItems",
                columns: new[] { "Status", "NextReviewAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyAttempts_StudyItemId_AttemptedAt",
                table: "StudyAttempts",
                columns: new[] { "StudyItemId", "AttemptedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyAttempts");

            migrationBuilder.DropIndex(
                name: "IX_StudyItems_Status_NextReviewAt_Id",
                table: "StudyItems");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "StudyItems");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "StudyItems");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "StudyItems");

            migrationBuilder.DropColumn(
                name: "NextReviewAt",
                table: "StudyItems");
        }
    }
}
