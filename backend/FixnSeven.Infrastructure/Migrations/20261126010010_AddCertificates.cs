using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    TechnicianId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grade = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Observations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublicSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    VerificationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerCodeHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.UniqueConstraint("AK_Certificates_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_Certificates_Expires", "\"ExpiresAt\" > \"IssuedAt\"");
                    table.CheckConstraint("CK_Certificates_Grade", "\"Grade\" IN ('Excelente','Bueno','Regular','Reprobado')");
                    table.CheckConstraint("CK_Certificates_Revoked", "(\"Status\" = 'Revocado') = (\"RevokedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_Certificates_Score", "\"Score\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Certificates_Status", "\"Status\" IN ('Vigente','Revocado')");
                    table.ForeignKey(
                        name: "FK_Certificates_Employees_EnterpriseId_TechnicianId",
                        columns: x => new { x.EnterpriseId, x.TechnicianId },
                        principalTable: "Employees",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Equipment_EnterpriseId_EquipmentId",
                        columns: x => new { x.EnterpriseId, x.EquipmentId },
                        principalTable: "Equipment",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_ServiceOrders_EnterpriseId_ServiceOrderId",
                        columns: x => new { x.EnterpriseId, x.ServiceOrderId },
                        principalTable: "ServiceOrders",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EnterpriseId_EquipmentId",
                table: "Certificates",
                columns: new[] { "EnterpriseId", "EquipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EnterpriseId_ServiceOrderId",
                table: "Certificates",
                columns: new[] { "EnterpriseId", "ServiceOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EnterpriseId_TechnicianId",
                table: "Certificates",
                columns: new[] { "EnterpriseId", "TechnicianId" });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EquipmentId",
                table: "Certificates",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 'Vigente'");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_VerificationCode",
                table: "Certificates",
                column: "VerificationCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}
