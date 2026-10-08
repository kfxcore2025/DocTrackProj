using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DocTrackProj.Migrations
{
    /// <inheritdoc />
    public partial class InitialCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromOfficeId = table.Column<int>(type: "int", nullable: false),
                    ToOfficeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminDocuments_Offices_FromOfficeId",
                        column: x => x.FromOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdminDocuments_Offices_ToOfficeId",
                        column: x => x.ToOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromOfficeId = table.Column<int>(type: "int", nullable: false),
                    ToOfficeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Offices_FromOfficeId",
                        column: x => x.FromOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Offices_ToOfficeId",
                        column: x => x.ToOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromOfficeId = table.Column<int>(type: "int", nullable: false),
                    ToOfficeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDocuments_Offices_FromOfficeId",
                        column: x => x.FromOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserDocuments_Offices_ToOfficeId",
                        column: x => x.ToOfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "IsActive", "OfficeName", "RoomNumber" },
                values: new object[,]
                {
                    { 1, true, "Accounting", "104" },
                    { 2, true, "Accreditation and Quality Assurance", "135" },
                    { 3, true, "Admin Office", "102" },
                    { 4, true, "Assistant Campus Director's/Dean of Instruction", "108" },
                    { 5, true, "BAC Office", "109" },
                    { 6, true, "BSFI Department", "116" },
                    { 7, true, "BSIT Department", "113" },
                    { 8, true, "Campus Director", "101" },
                    { 9, true, "Campus Secretary", "103" },
                    { 10, true, "Canteen", "139" },
                    { 11, true, "Cashier", "105" },
                    { 12, true, "Civil Security Services", "137" },
                    { 13, true, "Clinic", "120" },
                    { 14, true, "COA", "106" },
                    { 15, true, "College of Education", "114" },
                    { 16, true, "College of Technology & Engineering", "115" },
                    { 17, true, "DOST", "128" },
                    { 18, true, "Extension Services", "129" },
                    { 19, true, "Gender and Development", "121" },
                    { 20, true, "Graduate Studies", "119" },
                    { 21, true, "Guidance Counselor", "133" },
                    { 22, true, "HM Department", "117" },
                    { 23, true, "Hometel", "126" },
                    { 24, true, "HRMO", "107" },
                    { 25, true, "IA/SIAP (MAAM MANTOS)", "138" },
                    { 26, true, "ICPA", "134" },
                    { 27, true, "Library", "124" },
                    { 28, true, "Maintenance/Carpentry", "122" },
                    { 29, true, "MIS Assessment", "111" },
                    { 30, true, "MIS Office", "110" },
                    { 31, true, "NSTP", "130" },
                    { 32, true, "PACD/Guardhouse", "127" },
                    { 33, true, "Production Department", "132" },
                    { 34, true, "Research and Development", "113" },
                    { 35, true, "Resource Generation", "136" },
                    { 36, true, "Smart Room", "140" },
                    { 37, true, "SSG", "131" },
                    { 38, true, "Students Affairs Services", "125" },
                    { 39, true, "Supply Office", "123" },
                    { 40, true, "University Registrar", "112" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminDocuments_FromOfficeId",
                table: "AdminDocuments",
                column: "FromOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminDocuments_ToOfficeId",
                table: "AdminDocuments",
                column: "ToOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminDocuments_TransferKey",
                table: "AdminDocuments",
                column: "TransferKey");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FromOfficeId",
                table: "Documents",
                column: "FromOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ToOfficeId",
                table: "Documents",
                column: "ToOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_FromOfficeId",
                table: "UserDocuments",
                column: "FromOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_ToOfficeId",
                table: "UserDocuments",
                column: "ToOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_TransferKey",
                table: "UserDocuments",
                column: "TransferKey");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OfficeId",
                table: "Users",
                column: "OfficeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminDocuments");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "UserDocuments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Offices");
        }
    }
}
