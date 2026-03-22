using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImageGuidToExamQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageGuid",
                table: "ExamQuestions",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageGuid",
                table: "ExamQuestions");
        }
    }
}
