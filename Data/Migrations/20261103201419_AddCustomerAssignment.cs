using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcxiomCRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedUserId",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AssignedUserId",
                table: "Customers",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_AssignedUserId",
                table: "Customers",
                column: "AssignedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_AssignedUserId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_AssignedUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Customers");
        }
    }
}
