using JobLens.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobLens.Web.Migrations
{
    /// <summary>Groups skills into Technical (0), Personal (1) and Other (2). Existing skills become Technical.</summary>
    [DbContext(typeof(JobLensDbContext))]
    [Migration("20261013130000_AddSkillCategory")]
    public partial class AddSkillCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Skills");
        }
    }
}
