using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDayApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "users",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Events",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Events");
        }
    }
}
