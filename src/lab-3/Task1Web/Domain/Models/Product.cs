namespace Task1Web.Domain.Models;

public class Product
{
    public long ProductId { get; set; }

    public string ProductName { get; set; }

    public decimal ProductPrice { get; set; }

    public Product(long productId, string productName, decimal productPrice)
    {
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
    }
}