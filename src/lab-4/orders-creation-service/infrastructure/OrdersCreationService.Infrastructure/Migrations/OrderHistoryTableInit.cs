using FluentMigrator;

namespace OrdersCreationService.Infrastructure.Migrations;

[Migration(271020250004)]
public class OrderHistoryTableInit : Migration
{
    public override void Up()
    {
        Execute.Sql("create type order_history_item_kind as enum ('created', 'item_added', 'item_removed', 'state_changed');");
        Execute.Sql("""
                    create table if not exists order_history
                    (
                        order_history_item_id         bigint primary key generated always as identity,
                        order_id                      bigint                   not null references orders (order_id),
                    
                        order_history_item_created_at timestamp with time zone not null,
                        order_history_item_kind       order_history_item_kind  not null,
                        order_history_item_payload    jsonb                    not null
                    );
                    """);
    }

    public override void Down()
    {
        Execute.Sql("drop table if exists order_history");
        Execute.Sql("drop type order_history_item_kind");
    }
}