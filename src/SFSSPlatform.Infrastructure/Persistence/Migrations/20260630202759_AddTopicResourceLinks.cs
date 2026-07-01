using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFSSPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicResourceLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopicResourceLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    LearningResourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    VideoCandidateId = table.Column<int>(type: "INTEGER", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicResourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicResourceLinks_LearningResources_LearningResourceId",
                        column: x => x.LearningResourceId,
                        principalTable: "LearningResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopicResourceLinks_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopicResourceLinks_VideoCandidates_VideoCandidateId",
                        column: x => x.VideoCandidateId,
                        principalTable: "VideoCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopicResourceLinks_LearningResourceId",
                table: "TopicResourceLinks",
                column: "LearningResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicResourceLinks_TopicId",
                table: "TopicResourceLinks",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicResourceLinks_TopicId_LearningResourceId",
                table: "TopicResourceLinks",
                columns: new[] { "TopicId", "LearningResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicResourceLinks_TopicId_VideoCandidateId",
                table: "TopicResourceLinks",
                columns: new[] { "TopicId", "VideoCandidateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicResourceLinks_VideoCandidateId",
                table: "TopicResourceLinks",
                column: "VideoCandidateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicResourceLinks");
        }
    }
}
