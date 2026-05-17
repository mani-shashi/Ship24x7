using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ship24X7.Shipment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHubAssignmentOtpAndStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. CurrentHubId on Shipments ──────────────────────────────────
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentHubId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CurrentHubId",
                table: "Shipments",
                column: "CurrentHubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Hubs_CurrentHubId",
                table: "Shipments",
                column: "CurrentHubId",
                principalTable: "Hubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── 2. Delivery OTP fields on Shipments ───────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "DeliveryOtpHash",
                table: "Shipments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "Shipments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAgentId",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // ── 3. ShipmentStatusHistory table ────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ShipmentStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HubId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    // BaseEntity audit fields
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentStatusHistory_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistory_ShipmentId",
                table: "ShipmentStatusHistory",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistory_ChangedBy",
                table: "ShipmentStatusHistory",
                column: "ChangedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ShipmentStatusHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Hubs_CurrentHubId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_CurrentHubId",
                table: "Shipments");

            migrationBuilder.DropColumn(name: "CurrentHubId",      table: "Shipments");
            migrationBuilder.DropColumn(name: "DeliveryOtpHash",   table: "Shipments");
            migrationBuilder.DropColumn(name: "OtpExpiresAt",      table: "Shipments");
            migrationBuilder.DropColumn(name: "DeliveryAgentId",   table: "Shipments");
        }
    }
}
