namespace ShengTaOrderListing.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomersID { get; set; }
    public string? CustomersName { get; set; }
    public string? OrderD { get; set; } // JSON格式的订单详情
    public string GroupName { get; set; }
    public string location { get; set; }
    public float Totalamount { get; set; }
    public CityValue? City { get; set; }
    public string CompanyName { get; set; }
    public string RegistrationNumber { get; set; }
    public string TinNumber { get; set; }
    public string CompanyAddress { get; set; }
}

public class OrderItemDetail
{
    public bool IsGroupHeader { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? MarketPrice { get; set; }
    public decimal? MemberPrice { get; set; }
    public int? MaxOrder { get; set; }
    public string Company { get; set; }
    public bool HasOrder { get; set; }
    public bool IsSummaryRow { get; set; } = false;
    public int OrderCount { get; set; }
    public int CityTotal { get; set; }
    
}

public class CitySummary
{
    public CityValue? City { get; set; }
    public int TotalOrders { get; set; }
    public float TotalAmount { get; set; }
}

public class CityProductSummary
{
    public CityValue City { get; set; }
    public string ProductName { get; set; }
    public int TotalQuantity { get; set; }
}