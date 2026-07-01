using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingExerciseCheckDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckDefinitionJson",
                table: "CodingExercises",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckDefinitionJson",
                table: "CodingExercises");
        }
    }
}
