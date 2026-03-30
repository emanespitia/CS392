using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yummiez.Data.Migrations.YummiezDb
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safeguard for Azure DB: drop the table if it already exists from a previous iteration
            // We must drop any FK constraints that reference the Orders table, then drop the table itself
            migrationBuilder.Sql(@"
                IF OBJECT_ID('Orders', 'U') IS NOT NULL 
                BEGIN 
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' +  QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                    FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID('Orders');
                    EXEC sp_executesql @sql;
                    
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_Orders_Restaurants_restaurant_id]') AND parent_object_id = OBJECT_ID(N'[Orders]'))
                    BEGIN
                        ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Restaurants_restaurant_id];
                    END
                    DROP TABLE [Orders]; 
                END");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    restaurant_id = table.Column<int>(type: "int", nullable: false),
                    delivery_address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    driver_lat = table.Column<double>(type: "float", nullable: false),
                    driver_lng = table.Column<double>(type: "float", nullable: false),
                    dest_lat = table.Column<double>(type: "float", nullable: false),
                    dest_lng = table.Column<double>(type: "float", nullable: false),
                    restaurant_lat = table.Column<double>(type: "float", nullable: false),
                    restaurant_lng = table.Column<double>(type: "float", nullable: false),
                    driver_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    step_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_Orders_Restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "Restaurants",
                        principalColumn: "restaurant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_restaurant_id",
                table: "Orders",
                column: "restaurant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
