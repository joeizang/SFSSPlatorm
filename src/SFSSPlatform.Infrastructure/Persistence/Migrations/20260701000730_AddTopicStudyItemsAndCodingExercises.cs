using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicStudyItemsAndCodingExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SourceMaterialId",
                table: "StudyItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "SourceDocumentChunkId",
                table: "StudyItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "StudyItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodingExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 280, nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    StarterCode = table.Column<string>(type: "TEXT", nullable: false),
                    PackageRequirements = table.Column<string>(type: "TEXT", nullable: false),
                    SuccessCriteria = table.Column<string>(type: "TEXT", nullable: false),
                    Hints = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodingExercises_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudyItems_TopicId_Kind",
                table: "StudyItems",
                columns: new[] { "TopicId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_CodingExercises_TopicId_Title",
                table: "CodingExercises",
                columns: new[] { "TopicId", "Title" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyItems_Topics_TopicId",
                table: "StudyItems",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyItems_Topics_TopicId",
                table: "StudyItems");

            migrationBuilder.DropTable(
                name: "CodingExercises");

            migrationBuilder.DropIndex(
                name: "IX_StudyItems_TopicId_Kind",
                table: "StudyItems");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "StudyItems");

            migrationBuilder.AlterColumn<int>(
                name: "SourceMaterialId",
                table: "StudyItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SourceDocumentChunkId",
                table: "StudyItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
