using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueBookForTeachers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueBooks_Books_BookId",
                table: "IssueBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueBooks_Students_StudentId",
                table: "IssueBooks");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "IssueBooks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "IssueType",
                table: "IssueBooks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "IssueBooks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueBooks_TeacherId",
                table: "IssueBooks",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueBooks_Books_BookId",
                table: "IssueBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueBooks_Students_StudentId",
                table: "IssueBooks",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueBooks_Teachers_TeacherId",
                table: "IssueBooks",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueBooks_Books_BookId",
                table: "IssueBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueBooks_Students_StudentId",
                table: "IssueBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueBooks_Teachers_TeacherId",
                table: "IssueBooks");

            migrationBuilder.DropIndex(
                name: "IX_IssueBooks_TeacherId",
                table: "IssueBooks");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "IssueBooks");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "IssueBooks");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "IssueBooks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueBooks_Books_BookId",
                table: "IssueBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueBooks_Students_StudentId",
                table: "IssueBooks",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
