using FluentMigrator;

namespace OrdersCreationService.Infrastructure.Migrations;

[Migration(271020250002)]
public class OrdersTableInit : Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    create type order_state as enum ('created', 'processing', 'completed', 'cancelled');
                    """);

        Execute.Sql("""
                    create table if not exists orders
                    (
                        order_id         bigint primary key generated always as identity,
                    
                        order_state      order_state              not null,
                        order_created_at timestamp with time zone not null,
                        order_created_by text                     not null
                    );
                    """);
    }

    public override void Down()
    {
        Execute.Sql("drop table if exists orders;");
        Execute.Sql("drop type order_state;");
    }
}