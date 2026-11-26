using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    IdentifierLabel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UsageUnit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategories", x => x.Id);
                    table.UniqueConstraint("AK_EquipmentCategories_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_EquipmentCategories_UsageUnit", "\"UsageUnit\" IN ('Fechas','Horas')");
                    table.ForeignKey(
                        name: "FK_EquipmentCategories_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Identifier = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Brand = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Model = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CurrentUsage = table.Column<int>(type: "integer", nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.UniqueConstraint("AK_Equipment_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_Equipment_CurrentUsage", "\"CurrentUsage\" IS NULL OR \"CurrentUsage\" >= 0");
                    table.ForeignKey(
                        name: "FK_Equipment_Customers_EnterpriseId_CustomerId",
                        columns: x => new { x.EnterpriseId, x.CustomerId },
                        principalTable: "Customers",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentCategories_EnterpriseId_CategoryId",
                        columns: x => new { x.EnterpriseId, x.CategoryId },
                        principalTable: "EquipmentCategories",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EnterpriseId_CategoryId",
                table: "Equipment",
                columns: new[] { "EnterpriseId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EnterpriseId_CustomerId",
                table: "Equipment",
                columns: new[] { "EnterpriseId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EnterpriseId_Identifier",
                table: "Equipment",
                columns: new[] { "EnterpriseId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_EnterpriseId_Name",
                table: "EquipmentCategories",
                columns: new[] { "EnterpriseId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "EquipmentCategories");
        }
    }
}
