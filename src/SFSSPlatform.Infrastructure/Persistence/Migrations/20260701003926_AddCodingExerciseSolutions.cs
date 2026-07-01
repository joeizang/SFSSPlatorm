using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingExerciseSolutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodingExerciseSolutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodingExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastCheckedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingExerciseSolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodingExerciseSolutions_CodingExercises_CodingExerciseId",
                        column: x => x.CodingExerciseId,
                        principalTable: "CodingExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodingExerciseSolutions_CodingExerciseId",
                table: "CodingExerciseSolutions",
                column: "CodingExerciseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodingExerciseSolutions");
        }
    }
}
