using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace testconsole.Migrations
{
    public partial class InitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(nullable: false),
                    UserPassword = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Grade = table.Column<int>(nullable: true),
                    Major = table.Column<string>(nullable: true),
                    GraduateLevel = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UserInfoId);
                });

            migrationBuilder.CreateTable(
                name: "IMContactInfo",
                columns: table => new
                {
                    IMContactInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(nullable: true),
                    ProductProvider = table.Column<string>(nullable: true),
                    ContactAccount = table.Column<string>(nullable: true),
                    UserInfoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMContactInfo", x => x.IMContactInfoId);
                    table.ForeignKey(
                        name: "FK_IMContactInfo_users_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "users",
                        principalColumn: "UserInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IMContactInfo_UserInfoId",
                table: "IMContactInfo",
                column: "UserInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IMContactInfo");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
