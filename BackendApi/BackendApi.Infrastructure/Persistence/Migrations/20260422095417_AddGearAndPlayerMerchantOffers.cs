using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGearAndPlayerMerchantOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMerchantAvailable",
                table: "CardDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GearDefinitions",
                columns: table => new
                {
                    GearDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Slot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    ArmorValue = table.Column<int>(type: "int", nullable: false),
                    SetKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IconKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsMerchantAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearDefinitions", x => x.GearDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "GearSetDefinitions",
                columns: table => new
                {
                    GearSetDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ThreePieceBonusDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GearSetDefinitions", x => x.GearSetDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMerchantCardOffers",
                columns: table => new
                {
                    PlayerMerchantCardOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSold = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMerchantCardOffers", x => x.PlayerMerchantCardOfferId);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantCardOffers_CardDefinitions_CardDefinitionId",
                        column: x => x.CardDefinitionId,
                        principalTable: "CardDefinitions",
                        principalColumn: "CardDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantCardOffers_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantCardOffers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGears",
                columns: table => new
                {
                    PlayerGearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GearDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcquiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGears", x => x.PlayerGearId);
                    table.ForeignKey(
                        name: "FK_PlayerGears_GearDefinitions_GearDefinitionId",
                        column: x => x.GearDefinitionId,
                        principalTable: "GearDefinitions",
                        principalColumn: "GearDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerGears_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMerchantGearOffers",
                columns: table => new
                {
                    PlayerMerchantGearOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GearDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsSold = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMerchantGearOffers", x => x.PlayerMerchantGearOfferId);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantGearOffers_GearDefinitions_GearDefinitionId",
                        column: x => x.GearDefinitionId,
                        principalTable: "GearDefinitions",
                        principalColumn: "GearDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantGearOffers_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "MerchantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMerchantGearOffers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GearDefinitions_Key",
                table: "GearDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GearSetDefinitions_SetKey",
                table: "GearSetDefinitions",
                column: "SetKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGears_GearDefinitionId",
                table: "PlayerGears",
                column: "GearDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGears_PlayerId",
                table: "PlayerGears",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantCardOffers_CardDefinitionId",
                table: "PlayerMerchantCardOffers",
                column: "CardDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantCardOffers_MerchantId",
                table: "PlayerMerchantCardOffers",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantCardOffers_PlayerId_MerchantId_DisplayOrder",
                table: "PlayerMerchantCardOffers",
                columns: new[] { "PlayerId", "MerchantId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantGearOffers_GearDefinitionId",
                table: "PlayerMerchantGearOffers",
                column: "GearDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantGearOffers_MerchantId",
                table: "PlayerMerchantGearOffers",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMerchantGearOffers_PlayerId_MerchantId_DisplayOrder",
                table: "PlayerMerchantGearOffers",
                columns: new[] { "PlayerId", "MerchantId", "DisplayOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GearSetDefinitions");

            migrationBuilder.DropTable(
                name: "PlayerGears");

            migrationBuilder.DropTable(
                name: "PlayerMerchantCardOffers");

            migrationBuilder.DropTable(
                name: "PlayerMerchantGearOffers");

            migrationBuilder.DropTable(
                name: "GearDefinitions");

            migrationBuilder.DropColumn(
                name: "IsMerchantAvailable",
                table: "CardDefinitions");
        }
    }
}
