using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixnSeven.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerCodeUsedAt",
                table: "Certificates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailedLinkAttempts = table.Column<int>(type: "integer", nullable: false),
                    LinkLockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Accessories = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PublicDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrivateNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShowInPublicView = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransferCodeHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransferExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDevices", x => x.Id);
                    table.CheckConstraint("CK_CustomerDevices_Status", "\"Status\" IN ('Activo','Transferido')");
                    table.ForeignKey(
                        name: "FK_CustomerDevices_CustomerAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerDevices_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDeviceFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileKind = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDeviceFiles", x => x.Id);
                    table.CheckConstraint("CK_CustomerDeviceFiles_Kind", "\"FileKind\" IN ('Foto','PDF')");
                    table.CheckConstraint("CK_CustomerDeviceFiles_PublicPhoto", "NOT \"IsPublic\" OR \"FileKind\" = 'Foto'");
                    table.ForeignKey(
                        name: "FK_CustomerDeviceFiles_CustomerDevices_CustomerDeviceId",
                        column: x => x.CustomerDeviceId,
                        principalTable: "CustomerDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccounts_Email",
                table: "CustomerAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeviceFiles_CustomerDeviceId",
                table: "CustomerDeviceFiles",
                column: "CustomerDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDevices_CustomerAccountId",
                table: "CustomerDevices",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDevices_EquipmentId",
                table: "CustomerDevices",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 'Activo'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDeviceFiles");

            migrationBuilder.DropTable(
                name: "CustomerDevices");

            migrationBuilder.DropTable(
                name: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "OwnerCodeUsedAt",
                table: "Certificates");
        }
    }
}
