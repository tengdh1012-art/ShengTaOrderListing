using Dapper;
using MySqlConnector;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ShengTaOrderListing.Models;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using static MudBlazor.Icons;
using static MudBlazor.TimeSeriesChartSeries;

namespace ShengTaOrderListing.Services;

public class CustomerService
{
    private readonly string _connectionString;

    public CustomerService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task AddCustomer(Customer customer)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        string formattedIC = FormatIC(customer.IC);
        string formattedPhone = FormatPhoneNumber(customer.PhoneNumber);
        var parameters = new
        {
            customer.CustomersName,
            customer.CustomersID,
            IC = formattedIC,
            PhoneNumber = formattedPhone,
            customer.Location,
            customer.TypeCrops,
            customer.PlatingArea,
            customer.CompanyName,
            customer.RegistrationNumber,
            customer.TinNumber,
            customer.CompanyAddress,
            customer.City
        };

        var sql = @"INSERT INTO customerdetails 
                (CustomersName, CustomersID, IC, PhoneNumber, Location,TypeCrops,PlatingArea,CompanyName,RegistrationNumber,TinNumber,CompanyAddress,City) 
                VALUES 
                (@CustomersName, @CustomersID, @IC, @PhoneNumber, @Location,@TypeCrops,@PlatingArea,@CompanyName,@RegistrationNumber,@TinNumber,@CompanyAddress,@City);
                
                SELECT LAST_INSERT_ID();";

        var id = await connection.ExecuteScalarAsync<long>(sql, parameters);
        customer.Id = (int)id;
    }

    public async Task<int> GetMaxCustomerID()
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("SELECT MAX(customersID) FROM customerdetails", conn);
        var result = await cmd.ExecuteScalarAsync();

        if (result == DBNull.Value || result == null)
            return 0;

        return Convert.ToInt32(result);
    }

    private string FormatIC(string ic)
    {
        // 去掉非数字字符
        ic = new string(ic.Where(char.IsDigit).ToArray());

        if (ic.Length == 12)
        {
            return $"{ic.Substring(0, 6)}-{ic.Substring(6, 2)}-{ic.Substring(8, 4)}";
        }

        return ic; // 长度不对就不处理
    }

    private string FormatPhoneNumber(string phone)
    {
        // 去掉非数字字符
        phone = new string(phone.Where(char.IsDigit).ToArray());

        if (phone.Length == 10 || phone.Length == 11)
        {
            return $"{phone.Substring(0, 3)}-{phone.Substring(3)}";
        }

        return phone; // 长度不对就不处理
    }


    //public async Task<IEnumerable<Customer>> GetCustomers()
    //{
    //    using var conn = new MySqlConnection(_connectionString);
    //    await conn.OpenAsync();

    //    var cmd = new MySqlCommand("SELECT * FROM Customers", conn);
    //    using var reader = await cmd.ExecuteReaderAsync();

    //    var customers = new List<Customer>();
    //    while (await reader.ReadAsync())
    //    {
    //        customers.Add(new Customer
    //        {
    //            Id = reader.GetInt32("Id"),
    //            Name = reader.GetString("Name"),
    //            Email = reader.GetString("Email"),
    //            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString("Phone"),
    //            RegistrationDate = reader.GetDateTime("RegistrationDate")
    //        });
    //    }
    //    return customers;
    //}

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM customerdetails WHERE 1=1";
        var parameters = new DynamicParameters();

        sql += " ORDER BY customersID";

        try
        {
            return (await connection.QueryAsync<Customer>(sql, parameters)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail to search for customers: {ex.Message}");
            throw;
        }
    }



    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);

            // 使用正确的字段名（检查数据库实际字段名）
            const string sql = "SELECT * FROM customerdetails WHERE CustomersID = @Id";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            return null; // 或抛出特定异常
        }
    }

    private readonly List<Customer> _customers = new List<Customer>();

    public async Task DeleteCustomerAsync(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM customerdetails WHERE id = @Id";
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }

    public async Task<List<Customer>> SearchCustomersAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM Customers WHERE 1=1";
        var parameters = new DynamicParameters();

        //if (!string.IsNullOrWhiteSpace(name))
        //{
        //    sql += " AND Name LIKE @Name";
        //    parameters.Add("Name", $"%{name}%");
        //}

        //if (!string.IsNullOrWhiteSpace(email))
        //{
        //    sql += " AND Email LIKE @Email";
        //    parameters.Add("Email", $"%{email}%");
        //}

        //if (!string.IsNullOrWhiteSpace(phone))
        //{
        //    sql += " AND Phone LIKE @Phone";
        //    parameters.Add("Phone", $"%{phone}%");
        //}

        sql += " ORDER BY Id";
        Console.WriteLine(sql);

        try
        {
            return (await connection.QueryAsync<Customer>(sql, parameters)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail to search for customers: {ex.Message}");
            throw;
        }


    }


}