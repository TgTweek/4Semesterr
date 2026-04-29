using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryLocationAndLoadout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropIndex(
                name: "IX_PlayerGears_PlayerId",
                table: "PlayerGears");

            migrationBuilder.DropIndex(
                name: "IX_PlayerCards_PlayerId",
                table: "PlayerCards");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PlayerGears",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Stash");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PlayerGears",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "LoadoutOrder",
                table: "PlayerCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PlayerCards",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Stash");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PlayerCards",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGears_PlayerId_Location",
                table: "PlayerGears",
                columns: new[] { "PlayerId", "Location" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_PlayerId_LoadoutOrder",
                table: "PlayerCards",
                columns: new[] { "PlayerId", "LoadoutOrder" },
                unique: true,
                filter: "[LoadoutOrder] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_PlayerId_Location",
                table: "PlayerCards",
                columns: new[] { "PlayerId", "Location" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerGears_PlayerId_Location",
                table: "PlayerGears");

            migrationBuilder.DropIndex(
                name: "IX_PlayerCards_PlayerId_LoadoutOrder",
                table: "PlayerCards");

            migrationBuilder.DropIndex(
                name: "IX_PlayerCards_PlayerId_Location",
                table: "PlayerCards");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PlayerGears");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PlayerGears");

            migrationBuilder.DropColumn(
                name: "LoadoutOrder",
                table: "PlayerCards");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PlayerCards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PlayerCards");

           

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGears_PlayerId",
                table: "PlayerGears",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_PlayerId",
                table: "PlayerCards",
                column: "PlayerId");

           

           
        }
    }
}
