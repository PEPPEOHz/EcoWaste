using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

#nullable disable

namespace EcoWaste.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_USERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Username = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_USERS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "COLLECTION_POINTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Longitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    CapacityKg = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    AlertThresholdPercent = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Active = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COLLECTION_POINTS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "COLLECTION_ALERTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CollectionPointId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CurrentLoadKg = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    OccupancyPercent = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Message = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Resolved = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COLLECTION_ALERTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_COLLECTION_ALERTS_COLLECTION_POINTS_CollectionPointId",
                        column: x => x.CollectionPointId,
                        principalTable: "COLLECTION_POINTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WASTE_RECORDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CollectionPointId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    WeightKg = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Collected = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WASTE_RECORDS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WASTE_RECORDS_COLLECTION_POINTS_CollectionPointId",
                        column: x => x.CollectionPointId,
                        principalTable: "COLLECTION_POINTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APP_USERS_Username",
                table: "APP_USERS",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_COLLECTION_ALERTS_CollectionPointId",
                table: "COLLECTION_ALERTS",
                column: "CollectionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_COLLECTION_ALERTS_Resolved",
                table: "COLLECTION_ALERTS",
                column: "Resolved");

            migrationBuilder.CreateIndex(
                name: "IX_COLLECTION_POINTS_Name",
                table: "COLLECTION_POINTS",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WASTE_RECORDS_CollectionPointId_Collected",
                table: "WASTE_RECORDS",
                columns: new[] { "CollectionPointId", "Collected" });

            migrationBuilder.CreateIndex(
                name: "IX_WASTE_RECORDS_RecordedAt",
                table: "WASTE_RECORDS",
                column: "RecordedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "APP_USERS");
            migrationBuilder.DropTable(name: "COLLECTION_ALERTS");
            migrationBuilder.DropTable(name: "WASTE_RECORDS");
            migrationBuilder.DropTable(name: "COLLECTION_POINTS");
        }
    }
}
