using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustedYouTubeChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustedYouTubeChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedYouTubeChannels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustedYouTubeChannels_Priority",
                table: "TrustedYouTubeChannels",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedYouTubeChannels_Tags",
                table: "TrustedYouTubeChannels",
                column: "Tags");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedYouTubeChannels_Url",
                table: "TrustedYouTubeChannels",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustedYouTubeChannels");
        }
    }
}
