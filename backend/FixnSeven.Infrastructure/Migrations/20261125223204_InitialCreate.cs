using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnterpriseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DefaultCertificateDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MonthlyPriceUsd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: true),
                    MaxMonthlyOrders = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enterprises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnterpriseTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateValidityDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enterprises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enterprises_EnterpriseTypes_EnterpriseTypeId",
                        column: x => x.EnterpriseTypeId,
                        principalTable: "EnterpriseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enterprises_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    EnterpriseId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.UniqueConstraint("AK_Customers_EnterpriseId_Id", x => new { x.EnterpriseId, x.Id });
                    table.ForeignKey(
                        name: "FK_Customers_Enterprises_EnterpriseId",
                        column: x => x.EnterpriseId,
                        principalTable: "Enterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EnterpriseTypes",
                columns: new[] { "Id", "Code", "DefaultCertificateDays", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("0192f000-0000-7000-8000-000000000101"), "Workshop", 30, "Recibe, repara y devuelve equipos", "Taller de reparación" },
                    { new Guid("0192f000-0000-7000-8000-000000000102"), "UsedEquipmentDistributor", 30, "Compra, revisa, reacondiciona y certifica equipos", "Distribuidor de equipos usados" }
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "MaxMonthlyOrders", "MaxUsers", "MonthlyPriceUsd", "Name" },
                values: new object[,]
                {
                    { new Guid("0192f000-0000-7000-8000-000000000201"), 200, 50, 1500m, "Básico" },
                    { new Guid("0192f000-0000-7000-8000-000000000202"), null, 100, 2000m, "Profesional" },
                    { new Guid("0192f000-0000-7000-8000-000000000203"), null, null, 2500m, "Taller+" }
                });

            migrationBuilder.InsertData(
                table: "RoleEmployees",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("0192f000-0000-7000-8000-000000000301"), null, "Admin" },
                    { new Guid("0192f000-0000-7000-8000-000000000302"), null, "Reception" },
                    { new Guid("0192f000-0000-7000-8000-000000000303"), null, "Technician" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_EnterpriseTypeId",
                table: "Enterprises",
                column: "EnterpriseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Enterprises_PlanId",
                table: "Enterprises",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseTypes_Code",
                table: "EnterpriseTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleEmployees_Name",
                table: "RoleEmployees",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "RoleEmployees");

            migrationBuilder.DropTable(
                name: "Enterprises");

            migrationBuilder.DropTable(
                name: "EnterpriseTypes");

            migrationBuilder.DropTable(
                name: "Plans");
        }
    }
}
