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

    private bool _needOrder = false;
    public string CompanyName { get; set; }
    public string RegistrationNumber { get; set; }
    public string TinNumber { get; set; }  
    public string CompanyAddress { get; set; }
    public CityValue City { get; set; }

}

public enum CityValue { Bukit_Gambir, Tangkak, Simpang_Renggam,Pagoh, Penang, Kuala_Lumpur,Yong_Peng }
