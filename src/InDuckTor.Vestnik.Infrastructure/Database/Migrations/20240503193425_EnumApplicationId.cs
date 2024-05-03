using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InDuckTor.Vestnik.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class EnumApplicationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                alter table vestnik.client_app_registrations
                    add column int_application_id integer not null default 1;
                
                update vestnik.client_app_registrations
                set int_application_id =
                        case
                            when application_id = 'inductorbank' then 1
                            when application_id = 'employee_inductorbank' then 2
                            else 0
                            end;
                
                alter table vestnik.client_app_registrations
                    drop column application_id;
                alter table vestnik.client_app_registrations
                    rename column int_application_id to application_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
