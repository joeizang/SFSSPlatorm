using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    ChannelName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    ChannelUrl = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    EmbedUrl = table.Column<string>(type: "TEXT", maxLength: 800, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    LearningResourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoCandidates_LearningResources_LearningResourceId",
                        column: x => x.LearningResourceId,
                        principalTable: "LearningResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_ChannelName",
                table: "VideoCandidates",
                column: "ChannelName");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_Difficulty",
                table: "VideoCandidates",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_ExternalId",
                table: "VideoCandidates",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_LearningResourceId",
                table: "VideoCandidates",
                column: "LearningResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_Status",
                table: "VideoCandidates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VideoCandidates_Tags",
                table: "VideoCandidates",
                column: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoCandidates");
        }
    }
}
