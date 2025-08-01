namespace ShengTaOrderListing.Models;

public class Customer
{
    public int Id { get; set; }
    public int? CustomersID { get; set; }
    public string? CustomersName { get; set; }
    public string? IC { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? TypeCrops { get; set; }
    public string? PlatingArea { get; set; }
}