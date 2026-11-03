using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcxiomCRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadAssignmentAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Leads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Leads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Leads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AssignedUserId",
                table: "Leads",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_AspNetUsers_AssignedUserId",
                table: "Leads",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_AspNetUsers_AssignedUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_AssignedUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Leads");
        }
    }
}
