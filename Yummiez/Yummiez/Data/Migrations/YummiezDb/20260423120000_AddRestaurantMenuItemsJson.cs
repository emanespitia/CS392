using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Yummiez.Data;

#nullable disable

namespace Yummiez.Data.Migrations.YummiezDb
{
    [DbContext(typeof(YummiezDbContext))]
    [Migration("20260423120000_AddRestaurantMenuItemsJson")]
    public partial class AddRestaurantMenuItemsJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "menu_items_json",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "menu_items_json",
                table: "Restaurants");
        }
    }
}
