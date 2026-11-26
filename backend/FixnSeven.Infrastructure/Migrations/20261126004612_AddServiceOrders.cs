using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TechnicianId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ReportedIssue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReceivedCondition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkDone = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOrders", x => x.Id);
                    table.UniqueConstraint("AK_ServiceOrders_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_ServiceOrders_ClosedAt", "(\"Status\" = 'Cerrada') = (\"ClosedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_ServiceOrders_Status", "\"Status\" IN ('Recibida','EnProceso','Cerrada')");
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Customers_EnterpriseId_CustomerId",
                        columns: x => new { x.EnterpriseId, x.CustomerId },
                        principalTable: "Customers",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Employees_EnterpriseId_TechnicianId",
                        columns: x => new { x.EnterpriseId, x.TechnicianId },
                        principalTable: "Employees",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOrders_Equipment_EnterpriseId_EquipmentId",
                        columns: x => new { x.EnterpriseId, x.EquipmentId },
                        principalTable: "Equipment",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceItems", x => x.Id);
                    table.CheckConstraint("CK_ServiceItems_Kind", "\"Kind\" IN ('Trabajo','Repuesto')");
                    table.CheckConstraint("CK_ServiceItems_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_ServiceItems_UnitPrice", "\"UnitPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_ServiceItems_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceItems_ServiceOrders_EnterpriseId_ServiceOrderId",
                        columns: x => new { x.EnterpriseId, x.ServiceOrderId },
                        principalTable: "ServiceOrders",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceItems_EnterpriseId_ServiceOrderId",
                table: "ServiceItems",
                columns: new[] { "EnterpriseId", "ServiceOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_EnterpriseId_CustomerId",
                table: "ServiceOrders",
                columns: new[] { "EnterpriseId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_EnterpriseId_EquipmentId",
                table: "ServiceOrders",
                columns: new[] { "EnterpriseId", "EquipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_EnterpriseId_Status_CreatedAt",
                table: "ServiceOrders",
                columns: new[] { "EnterpriseId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_EnterpriseId_TechnicianId",
                table: "ServiceOrders",
                columns: new[] { "EnterpriseId", "TechnicianId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceItems");

            migrationBuilder.DropTable(
                name: "ServiceOrders");
        }
    }
}
