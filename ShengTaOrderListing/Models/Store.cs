namespace ShengTaOrderListing.Models;

public class Store
{
    public bool IsGroupHeader { get; set; }
    public int Id { get; set; }
    public string ProductName {  get; set; }
    public int? MaxOrder { get; set; }
    public OrderUnit Unit {  get; set; }
    public float? MarketPrice { get; set; } 
    public float? MemberPrice { get; set; } 
    public int Storeid { get; set; }
    public string Company { get; set; }
    public float Totalamount { get; set; }
}

public enum OrderUnit { BLT, BAG, DRUM, SETS, PACKS }
