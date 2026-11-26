using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterType = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ReporterEnterpriseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReporterName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ReporterEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ReporterPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Reason = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    TrackingCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.UniqueConstraint("AK_Reports_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.CheckConstraint("CK_Reports_Reason", "\"Reason\" IN ('EstadoDistinto','NivelIncorrecto','DatosErroneos','PosibleFalsificacion','Otro')");
                    table.CheckConstraint("CK_Reports_ReporterType", "\"ReporterType\" IN ('Cliente','Distribuidor')");
                    table.CheckConstraint("CK_Reports_Resolved", "(\"Status\" IN ('Aceptado','Rechazado')) = (\"ResolvedAt\" IS NOT NULL)");
                    table.CheckConstraint("CK_Reports_Status", "\"Status\" IN ('Recibido','EnRevision','Aceptado','Rechazado')");
                    table.ForeignKey(
                        name: "FK_Reports_Certificates_EnterpriseId_CertificateId",
                        columns: x => new { x.EnterpriseId, x.CertificateId },
                        principalTable: "Certificates",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Employees_EnterpriseId_ResolvedByEmployeeId",
                        columns: x => new { x.EnterpriseId, x.ResolvedByEmployeeId },
                        principalTable: "Employees",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Enterprises_ReporterEnterpriseId",
                        column: x => x.ReporterEnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileKind = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportAttachments", x => x.Id);
                    table.CheckConstraint("CK_ReportAttachments_Kind", "\"FileKind\" IN ('Foto','PDF','Video','Documento')");
                    table.CheckConstraint("CK_ReportAttachments_Size", "\"SizeBytes\" > 0");
                    table.ForeignKey(
                        name: "FK_ReportAttachments_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportAttachments_Reports_EnterpriseId_ReportId",
                        columns: x => new { x.EnterpriseId, x.ReportId },
                        principalTable: "Reports",
                        principalColumns: new[] { "EnterpriseId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportAttachments_EnterpriseId_ReportId",
                table: "ReportAttachments",
                columns: new[] { "EnterpriseId", "ReportId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CertificateId",
                table: "Reports",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EnterpriseId_CertificateId",
                table: "Reports",
                columns: new[] { "EnterpriseId", "CertificateId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EnterpriseId_ResolvedByEmployeeId",
                table: "Reports",
                columns: new[] { "EnterpriseId", "ResolvedByEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EnterpriseId_Status",
                table: "Reports",
                columns: new[] { "EnterpriseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterEmail_CreatedAt",
                table: "Reports",
                columns: new[] { "ReporterEmail", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterEnterpriseId",
                table: "Reports",
                column: "ReporterEnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TrackingCode",
                table: "Reports",
                column: "TrackingCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportAttachments");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
