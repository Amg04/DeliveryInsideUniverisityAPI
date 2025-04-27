using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AssignRolesToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    INSERT INTO [Security].[UserRoles] (UserId, RoleId)
                    SELECT 'bf3ff1a9-12b0-481a-86ac-be74f77fa6c6', Id
                    FROM [Security].[Roles];

                    INSERT INTO [Security].[UserRoles] (UserId, RoleId)
                    SELECT '055afc36-4522-44e5-91e0-eac098b9c7f6', Id
                    FROM [Security].[Roles]
                    WHERE Name = 'ExecutedOrder'; 

                    -- إعطاء المستخدم الثالث صلاحية Delivery
                    INSERT INTO [Security].[UserRoles] (UserId, RoleId)
                    SELECT '1c63e735-b932-4396-a323-52f18aec630b', Id
                    FROM [Security].[Roles]
                    WHERE Name = 'Delivery';

                    -- إعطاء المستخدم الرابع صلاحية Customer
                    INSERT INTO [Security].[UserRoles] (UserId, RoleId)
                    SELECT '4e4211d3-ce1e-45ff-9834-0b53c69ff388', Id
                    FROM [Security].[Roles]
                    WHERE Name = 'Customer';"
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [Security].[UserRoles]
                WHERE UserId IN (
                    'bf3ff1a9-12b0-481a-86ac-be74f77fa6c6', 
                    '055afc36-4522-44e5-91e0-eac098b9c7f6', 
                    '1c63e735-b932-4396-a323-52f18aec630b', 
                    '4e4211d3-ce1e-45ff-9834-0b53c69ff388'
                );
            ");
        }
    }
}
