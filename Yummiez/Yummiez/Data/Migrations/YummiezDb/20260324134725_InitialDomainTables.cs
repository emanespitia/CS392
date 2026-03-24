using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yummiez.Data.Migrations.YummiezDb
{
    /// <inheritdoc />
    public partial class InitialDomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Clients]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Clients](
                        [client_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [identity_user_id] NVARCHAR(450) NOT NULL,
                        [display_name] NVARCHAR(150) NULL,
                        [phone] NVARCHAR(20) NULL,
                        [is_active] BIT NOT NULL,
                        [created_at] DATETIME2 NOT NULL
                    );
                END;
                ELSE
                BEGIN
                    IF COL_LENGTH('dbo.Clients', 'identity_user_id') IS NULL
                        ALTER TABLE [dbo].[Clients] ADD [identity_user_id] NVARCHAR(450) NULL;
                    IF COL_LENGTH('dbo.Clients', 'display_name') IS NULL
                        ALTER TABLE [dbo].[Clients] ADD [display_name] NVARCHAR(150) NULL;
                    IF COL_LENGTH('dbo.Clients', 'phone') IS NULL
                        ALTER TABLE [dbo].[Clients] ADD [phone] NVARCHAR(20) NULL;
                    IF COL_LENGTH('dbo.Clients', 'is_active') IS NULL
                        ALTER TABLE [dbo].[Clients] ADD [is_active] BIT NOT NULL CONSTRAINT [DF_Clients_is_active] DEFAULT(1);
                    IF COL_LENGTH('dbo.Clients', 'created_at') IS NULL
                        ALTER TABLE [dbo].[Clients] ADD [created_at] DATETIME2 NOT NULL CONSTRAINT [DF_Clients_created_at] DEFAULT(SYSUTCDATETIME());
                END;

                IF OBJECT_ID(N'[dbo].[Drivers]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Drivers](
                        [driver_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [identity_user_id] NVARCHAR(450) NOT NULL,
                        [license_number] NVARCHAR(50) NULL,
                        [vehicle_type] NVARCHAR(50) NULL,
                        [is_available] BIT NOT NULL,
                        [created_at] DATETIME2 NOT NULL
                    );
                END;
                ELSE
                BEGIN
                    IF COL_LENGTH('dbo.Drivers', 'identity_user_id') IS NULL
                        ALTER TABLE [dbo].[Drivers] ADD [identity_user_id] NVARCHAR(450) NULL;
                    IF COL_LENGTH('dbo.Drivers', 'license_number') IS NULL
                        ALTER TABLE [dbo].[Drivers] ADD [license_number] NVARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.Drivers', 'vehicle_type') IS NULL
                        ALTER TABLE [dbo].[Drivers] ADD [vehicle_type] NVARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.Drivers', 'is_available') IS NULL
                        ALTER TABLE [dbo].[Drivers] ADD [is_available] BIT NOT NULL CONSTRAINT [DF_Drivers_is_available] DEFAULT(0);
                    IF COL_LENGTH('dbo.Drivers', 'created_at') IS NULL
                        ALTER TABLE [dbo].[Drivers] ADD [created_at] DATETIME2 NOT NULL CONSTRAINT [DF_Drivers_created_at] DEFAULT(SYSUTCDATETIME());
                END;

                IF OBJECT_ID(N'[dbo].[Restaurants]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Restaurants](
                        [restaurant_id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [name] NVARCHAR(100) NOT NULL,
                        [owner_name] NVARCHAR(100) NOT NULL,
                        [address] NVARCHAR(500) NOT NULL,
                        [phone] NVARCHAR(20) NULL,
                        [is_open] BIT NULL,
                        [admin_id] INT NOT NULL,
                        [category] NVARCHAR(50) NULL,
                        [image_url] NVARCHAR(500) NULL,
                        [created_at] DATETIME2 NULL,
                        [updated_at] DATETIME2 NULL
                    );
                END;

                IF COL_LENGTH('dbo.Clients', 'identity_user_id') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_identity_user_id' AND object_id = OBJECT_ID('dbo.Clients'))
                BEGIN
                    EXEC(N'
                        CREATE UNIQUE INDEX [IX_Clients_identity_user_id]
                        ON [dbo].[Clients]([identity_user_id])
                        WHERE [identity_user_id] IS NOT NULL;
                    ');
                END;

                IF COL_LENGTH('dbo.Drivers', 'identity_user_id') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Drivers_identity_user_id' AND object_id = OBJECT_ID('dbo.Drivers'))
                BEGIN
                    EXEC(N'
                        CREATE UNIQUE INDEX [IX_Drivers_identity_user_id]
                        ON [dbo].[Drivers]([identity_user_id])
                        WHERE [identity_user_id] IS NOT NULL;
                    ');
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Clients]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[Clients];
                END;

                IF OBJECT_ID(N'[dbo].[Drivers]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[Drivers];
                END;

                IF OBJECT_ID(N'[dbo].[Restaurants]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[Restaurants];
                END;
                """);
        }
    }
}
