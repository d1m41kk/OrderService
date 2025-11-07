using FluentMigrator;

namespace Task1Web.Infrastructure.Migrations;

[Migration(271020250003)]
public class OrderItemsTableInit : Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    create table if not exists order_items
                    (
                        order_item_id       bigint primary key generated always as identity,
                        order_id            bigint  not null references orders (order_id),
                        product_id          bigint  not null references products (product_id),
                    
                        order_item_quantity int     not null,
                        order_item_deleted  boolean not null
                    );
                    """);
    }

    public override void Down()
    {
        Execute.Sql("drop table order_items");
    }
}