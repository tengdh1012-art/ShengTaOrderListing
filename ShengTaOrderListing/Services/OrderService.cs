using Dapper;
using MySqlConnector;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ShengTaOrderListing.Models;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ShengTaOrderListing.Services;

public class OrderService
{
    private readonly string _connectionString;

    private readonly StoreService _storeService;
    private readonly CustomerService _customerService;
    public OrderService(IConfiguration config, StoreService storeService, CustomerService customerService)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
        _customerService = customerService;
        _storeService = storeService; // 添加这行
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT * FROM orderdetails WHERE CustomersID = @id";
        return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task<List<Order>> GetOrderByCityAsync(string city)
    {
        using var connection = new MySqlConnection(_connectionString);

        // 把 string 转 enum → 再转 int
        if (!Enum.TryParse<CityValue>(city, out var cityEnum))
            throw new ArgumentException($"无效的城市: {city}");
        int cityInt = (int)cityEnum;

        var sql = "SELECT * FROM orderdetails WHERE City = @City";
        var result = await connection.QueryAsync<Order>(sql, new { City = cityInt });
        return result.ToList();
    }

    public async Task<List<int?>> GetCustomersWithOrdersAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT DISTINCT CustomersID FROM orderdetails";
            return (await connection.QueryAsync<int?>(sql)).ToList();
        }
        catch (Exception ex)
        {
            return new List<int?>();
        }
    }

    public async Task AddOrderAsync(Order order)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new
        {
            order.CustomersID,
            order.CustomersName,
            order.OrderD,
            order.Totalamount,
            order.City,
        };

        var sql = @"INSERT INTO orderdetails 
                    (CustomersID, CustomersName, OrderD,TotalAmount,City) 
                    VALUES 
                    (@CustomersID, @CustomersName, @OrderD,@Totalamount,@City);
                    SELECT LAST_INSERT_ID();";

        var id = await connection.ExecuteScalarAsync<int>(sql, parameters);
        order.Id = id;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM orderdetails WHERE 1=1";
        var parameters = new DynamicParameters();

        sql += " ORDER BY Id";

        try
        {
            return (await connection.QueryAsync<Order>(sql, parameters)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail to search for customers: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteOrderAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM orderdetails WHERE id = @Id";
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }

    public async Task<byte[]> GenerateCustomerReportExcelAsync(List<Order> orders,int customerID)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Orders Report");
        var customer = await _customerService.GetCustomerByIdAsync(customerID);
        worksheet.Cells.Style.Font.Name = "Aptos Narrow";

        if (customer == null)
        {
            throw new Exception($"Customer with ID {customerID} not found.");
        }
        // 标题
        worksheet.Cells["B4"].Value = "2025 PPSM Members Day Order Listing";
        worksheet.Cells["B4:G5"].Merge = true;
        worksheet.Cells["B4:G5"].Style.Font.Size = 20;
        worksheet.Cells["B4:G5"].Style.Font.Bold = true;
        worksheet.Cells["B4:G5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells["B4:G5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells["B4:G5"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        worksheet.Cells["B6"].Value = "()";
        worksheet.Cells["B6:G6"].Merge = true;
        worksheet.Cells["B6:G6"].Style.Font.Size = 20;
        worksheet.Cells["B6:G6"].Style.Font.Bold = true;
        worksheet.Cells["B6:G6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells["B6:G6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells["B6:G6"].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

        // 获取所有产品并创建字典
        var allProducts = await _storeService.GetAllStoreAsync();
        var productDict = allProducts.ToDictionary(p => p.Storeid, p => p);
        

        // 重构分组逻辑 - 确保正确分组
        var groupedItems = orders
            .SelectMany(order =>
            {
                if (string.IsNullOrWhiteSpace(order.OrderD))
                    return Enumerable.Empty<OrderItemDetail>();

                try
                {
                    return JsonSerializer.Deserialize<List<OrderItemDetail>>(order.OrderD)
                        ?? new List<OrderItemDetail>();
                }
                catch
                {
                    return new List<OrderItemDetail>();
                }
            })
            .Where(item => productDict.ContainsKey(item.ProductId)) // 过滤无效产品
            .GroupBy(item => productDict[item.ProductId].Company)   // 按公司分组
            .Where(g => !string.IsNullOrEmpty(g.Key))               // 过滤空公司                                    // 按公司名排序
            .ToList();

        // 表头
        int row = 7;
        const int colStart = 2;
        string[] headers = { "ITEM NO", "PRODUCT NAME", "MAX ORDER", "MARKET PRICE", "MEMBER PRICE", "QUANTITY" };

        // 写入表头
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[row, colStart + i];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.White);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
        row++;

        int globalItemNo = 1; // 全局计数器

        foreach (var companyGroup in groupedItems)
        {
            row++;
            worksheet.Cells[row, colStart+1].Value = companyGroup.Key;
            worksheet.Cells[row, colStart + 1].Style.Font.Size = 11;
            worksheet.Cells[row, colStart + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, colStart + 1].Style.Fill.BackgroundColor.SetColor(Color.White);
            worksheet.Cells[row, colStart + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, colStart + 1].Style.Font.Bold = true;
            row++;

            foreach(var productGroup in companyGroup.GroupBy(x => x.ProductId))
            {
                var item = productGroup.First(); // 取一个样本项
                var product = productDict[item.ProductId];

                worksheet.Cells[row, colStart + 0].Value = globalItemNo++;
                worksheet.Cells[row, colStart + 1].Value = item.ProductName;
                worksheet.Cells[row, colStart + 2].Value = $"{item.MaxOrder} {item.Unit}";
                if (item.MarketPrice != null)
                {
                    var marketCell = worksheet.Cells[row, colStart + 3];
                    marketCell.Value = item.MarketPrice.Value;
                    marketCell.Style.Numberformat.Format = "\"RM\" #,##0.00";
                }
                else
                {
                    worksheet.Cells[row, colStart + 3].Value = "-";
                }

                if (item.MemberPrice != null)
                {
                    var memberCell = worksheet.Cells[row, colStart + 4];
                    memberCell.Value = item.MemberPrice.Value;
                    memberCell.Style.Numberformat.Format = "\"RM\" #,##0.00";
                }
                else
                {
                    worksheet.Cells[row, colStart + 4].Value = "-";
                }
                worksheet.Cells[row, colStart + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, colStart + 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                // 计算总数量（如果重复的话）
                worksheet.Cells[row, colStart + 5].Value = productGroup.Sum(x => x.Quantity);

                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = worksheet.Cells[row, colStart + c];
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cell.Style.Font.Size = 12;
                }

                row++;
            }

            
        }
        row++;

        worksheet.Cells[row, 2].Value = "Customer Details";
        worksheet.Cells[row, 2, row, 3].Merge = true;
        worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;
        worksheet.Cells[row, 2, row, 3].Style.Font.Size = 14;
        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        worksheet.Cells[row, 2, row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        row++;

        string[] labels = new[]
        {
            "Customer Name:",
            "IC:",
            "Phone Number:",
            "Location:",
            "Type of Crops:",
            "Plating Area:"
        };

        string[] values = new[]
        {
            customer.CustomersName,
            customer.IC,
            customer.PhoneNumber,
            customer.Location,
            customer.TypeCrops,
            customer.PlatingArea
        };

        for (int i = 0; i < labels.Length; i++)
        {
            worksheet.Cells[row, 2].Value = labels[i];
            worksheet.Cells[row, 2].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[row, 2].Style.Font.Size = 12;
            worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[row, 3].Value = values[i];
            worksheet.Cells[row, 3].Style.Font.Size = 12;
            worksheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            row++;
        }
        row++;
        worksheet.Cells[row, 2].Value = "开单";
        worksheet.Cells[row, 2, row, 3].Merge = true;
        worksheet.Cells[row, 2, row, 3].Style.Font.Bold = true;
        worksheet.Cells[row, 2, row, 3].Style.Font.Size = 14;
        worksheet.Cells[row, 2, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[row, 2, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        worksheet.Cells[row, 2, row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        row++;

        string[] invoces = new[]
        {
            "Company Name",
            "Registration Number",
            "Tin Number",
            "CompanyAddress",
        };

        string[] invocevalues = new[]
        {
            customer.CompanyName,
            customer.RegistrationNumber,
            customer.TinNumber,
            customer.CompanyAddress
        };

        if (!string.IsNullOrWhiteSpace(customer.CompanyName))
        {
            for (int i = 0; i < invoces.Length; i++)
            {
                worksheet.Cells[row, 2].Value = invoces[i];
                worksheet.Cells[row, 2].Style.Font.Bold = true;
                worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 2].Style.Font.Size = 12;
                worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                worksheet.Cells[row, 3].Value = invocevalues[i];
                worksheet.Cells[row, 3].Style.Font.Size = 12;
                worksheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;
            }
        }   
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }
}
