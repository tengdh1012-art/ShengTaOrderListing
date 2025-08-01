using Dapper;
using MudBlazor.Charts;
using MySqlConnector;
using Org.BouncyCastle.Bcpg;
using ShengTaOrderListing.Models;
using System.Net.Http;
using static MudBlazor.Icons;
using static MudBlazor.TimeSeriesChartSeries;

namespace ShengTaOrderListing.Services
{
    public class StoreService
    {
        private readonly string _connectionString;

        public StoreService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }
        public async Task AddStore(Store store)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string unit = store.Unit.ToString();

            if (!new[] { "BLT", "BAG", "DRUM", "SETS", "PACKS" }.Contains(unit))
            {
                unit = "BLT";
            }

            var parameters = new
            {
                store.ProductName,
                store.MaxOrder,
                Unit = unit,
                store.MarketPrice,
                store.MemberPrice,
                store.Storeid,
                store.Company,
            };

            var sql = @"INSERT INTO storedetails 
                (ProductName, MaxOrder,Unit, MarketPrice, MemberPrice,StoreID,Company) 
                VALUES 
                (@ProductName, @MaxOrder, @Unit, @MarketPrice, @MemberPrice,@Storeid,@Company);
                
                SELECT LAST_INSERT_ID();";

            var id = await connection.ExecuteScalarAsync<long>(sql, parameters);
            store.Id = (int)id;
        }

        public async Task<int> GetMaxStoreID()
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT MAX(StoreID) FROM storedetails", conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result == DBNull.Value || result == null)
                return 0;

            return Convert.ToInt32(result);
        }

        public async Task<List<Store>> GetAllStoreAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT * FROM storedetails";
            var parameters = new DynamicParameters();

            sql += " ORDER BY StoreID";

            try
            {
                return (await connection.QueryAsync<Store>(sql, parameters)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fail to search for customers: {ex.Message}");
                throw;
            }

        }

        public async Task<List<string>> GetAllCompaniesAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT DISTINCT Company FROM storedetails";
            return (await connection.QueryAsync<string>(sql)).ToList();
        }

        public async Task DeleteStoreAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM storedetails WHERE id = @Id";
                await connection.ExecuteAsync(query, new { Id = id });
            }
        }

        public async Task UpdateStoreAsync(Store store)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            string unit = store.Unit.ToString();

            if (!new[] { "BLT", "BAG", "DRUM", "SETS", "PACKS" }.Contains(unit))
            {
                unit = "BLT";
            }
            var parameters = new
            {
                store.Id,
                store.Company,
                store.ProductName,
                Unit = unit,
                store.MaxOrder,
                store.MarketPrice,
                store.MemberPrice,
                store.Storeid,
            };

            var sql = @"UPDATE storedetails 
                SET id = @Id, 
                    ProductName = @ProductName, 
                    MaxOrder = @MaxOrder, 
                    Unit = @Unit, 
                    MarketPrice = @MarketPrice,
                    MemberPrice = @MemberPrice,
                    Company = @Company,
                    Storeid =@Storeid
                WHERE id = @Id;";

            await connection.ExecuteAsync(sql, parameters);
        }
    }

}
