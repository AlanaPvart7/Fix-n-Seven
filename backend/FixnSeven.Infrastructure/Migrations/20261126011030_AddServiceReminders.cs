using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueUsage = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReminders", x => x.Id);
                    table.UniqueConstraint("AK_ServiceReminders_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_ServiceReminders_Due", "\"DueDate\" IS NOT NULL OR \"DueUsage\" IS NOT NULL");
                    table.CheckConstraint("CK_ServiceReminders_DueUsage", "\"DueUsage\" IS NULL OR \"DueUsage\" > 0");
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Equipment_EnterpriseId_EquipmentId",
                        columns: x => new { x.EnterpriseId, x.EquipmentId },
                        principalTable: "Equipment",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_EnterpriseId_DueDate",
                table: "ServiceReminders",
                columns: new[] { "EnterpriseId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_EnterpriseId_EquipmentId",
                table: "ServiceReminders",
                columns: new[] { "EnterpriseId", "EquipmentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceReminders");
        }
    }
}
