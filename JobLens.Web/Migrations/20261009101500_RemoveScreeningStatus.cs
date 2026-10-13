using JobLens.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobLens.Web.Migrations
{
    /// <summary>Moves applications off the retired Screening status (1) to Interview (2).</summary>
    [DbContext(typeof(JobLensDbContext))]
    [Migration("20261009101500_RemoveScreeningStatus")]
    public partial class RemoveScreeningStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "JobApplications" SET "Status" = 2 WHERE "Status" = 1;""");
            migrationBuilder.Sql("""UPDATE "StatusChanges" SET "From" = 2 WHERE "From" = 1;""");
            migrationBuilder.Sql("""UPDATE "StatusChanges" SET "To" = 2 WHERE "To" = 1;""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
