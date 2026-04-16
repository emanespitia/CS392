using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yummiez.Data.Migrations.YummiezDb
{
    /// <inheritdoc />
    public partial class AddManagerAndDriverOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "manager_user_id",
                table: "Restaurants",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "driver_user_id",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "manager_user_id",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "driver_user_id",
                table: "Orders");
        }
    }
}
