using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
             table: "Roles",
             schema: "Security",
             columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
             values: new object[] { Guid.NewGuid().ToString(), "Customer", "Customer".ToUpper(), Guid.NewGuid().ToString() }
             );

            migrationBuilder.InsertData(
             table: "Roles",
             schema: "Security",
             columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
             values: new object[] { Guid.NewGuid().ToString(), "Admin", "Admin".ToUpper(), Guid.NewGuid().ToString() }
             );

            migrationBuilder.InsertData(
           table: "Roles",
           schema: "Security",
           columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
           values: new object[] { Guid.NewGuid().ToString(), "ExecutedOrder", "ExecutedOrder".ToUpper(), Guid.NewGuid().ToString() }
           );

            migrationBuilder.InsertData(
          table: "Roles",
          schema: "Security",
          columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
          values: new object[] { Guid.NewGuid().ToString(), "Delivery", "Delivery".ToUpper(), Guid.NewGuid().ToString() }
          );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from [Security].[Roles]");
        }
    }
}
