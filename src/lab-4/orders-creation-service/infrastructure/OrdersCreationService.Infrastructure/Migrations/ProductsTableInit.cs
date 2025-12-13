using FluentMigrator;

namespace OrdersCreationService.Infrastructure.Migrations;

[Migration(271020250001)]
public class ProductsTableInit : Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    create table if not exists products
                    (
                        product_id    bigint primary key generated always as identity,
                        product_name  text  not null,
                        product_price money not null
                    );
                    """);
    }

    public override void Down()
    {
        Execute.Sql("drop table if exists products;");
    }
}