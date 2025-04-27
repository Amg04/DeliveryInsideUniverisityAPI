using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO [Security].[Users] ([Id], [Name], [Address], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES " +
                "(N'055afc36-4522-44e5-91e0-eac098b9c7f6', N'salah', N'mans', N'ss2222', N'SS2222', N'ss2222@email.com', N'SS2222@EMAIL.COM', 0, N'AQAAAAIAAYagAAAAEODjwtKRjagMvqSYRX5E7znb/BWcDwC7iVXnF+Bb+MtcetuZygf3JLmw/R93tK/uYA==', N'JNKVJP5CIZAVVNAGBQ2HBFJNUWSX3NPB', N'392a3afd-5b42-4e86-9c2d-78b4cce7ebc4', NULL, 0, 0, NULL, 1, 0)," +
               "(N'1c63e735-b932-4396-a323-52f18aec630b', N'zyad', N'mans', N'zz2222', N'ZZ2222', N'zz2222@email.com', N'ZZ2222@EMAIL.COM', 0, N'AQAAAAIAAYagAAAAEGUBy31UjRDE1Nu16u49RSyxUdToq+zXpbAyB4fLpUm6UbApmQAs7p7Nu86DS/G9Bw==', N'F7H3UFNCPMOAUS2PG4OF3VIJVZT6JIHN', N'7ffbd044-1433-44e4-b7b1-c6451b7691b4', NULL, 0, 0, NULL, 1, 0)," +
               "(N'4e4211d3-ce1e-45ff-9834-0b53c69ff388', N'ali', N'mans', N'ee2222', N'EE2222', N'ee2222@email.com', N'EE2222@EMAIL.COM', 0, N'AQAAAAIAAYagAAAAEH/s1vOSbzPk3cHM7g0I4oNSmshX24/fbff7XOjPbELTaqIYmQTElYTlc3kp7mG52w==', N'BT6RN4GMUL2QOUMVTTTSND7PT3IJ3YFG', N'b3579e0e-4e11-4dbc-8b2f-000b3af3e046', NULL, 0, 0, NULL, 1, 0)," +
               "(N'bf3ff1a9-12b0-481a-86ac-be74f77fa6c6', N'Ahmed', N'mans', N'aa2222', N'AA2222', N'aa2222@email.com', N'AA2222@EMAIL.COM', 0, N'AQAAAAIAAYagAAAAEJRGmNiqw1Jt7Okrx6jbX9xOwY3lc8+cxvdsZmNsFXmzgqKP05UpG41StF81w/Uflg==', N'VYEUVK5YZ2B7KBXTUFCRDG6HUUXKWLTB', N'f84afe63-0ff9-4df6-92a4-588152abb7e4', NULL, 0, 0, NULL, 1, 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [Security].[Users] WHERE [Id] IN (" +
                "N'055afc36-4522-44e5-91e0-eac098b9c7f6', " +
                "N'1c63e735-b932-4396-a323-52f18aec630b', " +
                "N'4e4211d3-ce1e-45ff-9834-0b53c69ff388', " +
                "N'bf3ff1a9-12b0-481a-86ac-be74f77fa6c6');");
        }
    }
}
