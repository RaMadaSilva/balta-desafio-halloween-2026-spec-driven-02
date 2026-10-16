using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shortener.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261016000000_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Links",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Code = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY"),
                TargetUrl = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                ExpiresAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                AccessCount = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Links", item => item.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Links_Code",
            table: "Links",
            column: "Code",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Links");
}
