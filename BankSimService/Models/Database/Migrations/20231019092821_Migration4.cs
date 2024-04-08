using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSimService.Migrations
{
    /// <inheritdoc />
    public partial class Migration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessorSessionId",
                table: "TransactionTb");

            migrationBuilder.DropColumn(
                name: "ProcessorTranId",
                table: "TransactionTb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessorSessionId",
                table: "TransactionTb",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessorTranId",
                table: "TransactionTb",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
