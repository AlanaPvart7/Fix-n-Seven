using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateGradeScales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Grade = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    MinScore = table.Column<int>(type: "integer", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateGradeScales", x => x.Id);
                    table.CheckConstraint("CK_GradeScale_Grade", "\"Grade\" IN ('Excelente','Bueno','Regular','Reprobado')");
                    table.CheckConstraint("CK_GradeScale_Range", "\"MinScore\" >= 0 AND \"MaxScore\" <= 100 AND \"MinScore\" <= \"MaxScore\"");
                    table.ForeignKey(
                        name: "FK_CertificateGradeScales_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Method = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.UniqueConstraint("AK_Payments_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_Payments_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_Payments_Method", "\"Method\" IN ('Efectivo','Tarjeta','Transferencia')");
                    table.ForeignKey(
                        name: "FK_Payments_Employees_EnterpriseId_ReceivedByEmployeeId",
                        columns: x => new { x.EnterpriseId, x.ReceivedByEmployeeId },
                        principalTable: "Employees",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_ServiceOrders_EnterpriseId_ServiceOrderId",
                        columns: x => new { x.EnterpriseId, x.ServiceOrderId },
                        principalTable: "ServiceOrders",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateGradeScales_EnterpriseId_Grade",
                table: "CertificateGradeScales",
                columns: new[] { "EnterpriseId", "Grade" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnterpriseId_PaidAt",
                table: "Payments",
                columns: new[] { "EnterpriseId", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnterpriseId_ReceivedByEmployeeId",
                table: "Payments",
                columns: new[] { "EnterpriseId", "ReceivedByEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnterpriseId_ServiceOrderId",
                table: "Payments",
                columns: new[] { "EnterpriseId", "ServiceOrderId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateGradeScales");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
