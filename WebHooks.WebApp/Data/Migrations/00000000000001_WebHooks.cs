using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHooks.WebApp.Data.Migrations
{
    public partial class WebHooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebHooks",
                columns: table => new
                {
                    User = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false),
                    RowVer = table.Column<int>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHooks", x => new { x.Id, x.User });
                });
            migrationBuilder.Sql(@"CREATE TRIGGER UpdateWebHooksRowVer AFTER UPDATE ON WebHooks
                                    BEGIN
                                        UPDATE WebHooks
                                        SET RowVer = RowVer + 1
                                        WHERE rowid = NEW.rowid;
                                    END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER UpdateWebHooksRowVer");
            migrationBuilder.DropTable(
                name: "WebHooks");
        }
    }
}
