using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankSimService.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "TransactionTb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "TransactionTb",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
}
