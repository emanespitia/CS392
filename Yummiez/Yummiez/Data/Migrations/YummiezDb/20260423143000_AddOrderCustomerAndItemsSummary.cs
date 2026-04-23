using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Yummiez.Data;

#nullable disable

namespace Yummiez.Data.Migrations.YummiezDb
{
    [DbContext(typeof(YummiezDbContext))]
    [Migration("20260423143000_AddOrderCustomerAndItemsSummary")]
    public partial class AddOrderCustomerAndItemsSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "items_summary",
                table: "Orders",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_name",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "items_summary",
                table: "Orders");
        }
    }
}
