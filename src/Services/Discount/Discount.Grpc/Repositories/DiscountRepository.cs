using Dapper;
using Discount.Grpc.Entities;
using Npgsql;

namespace Discount.Grpc.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> CreateDiscountAsync(Coupon coupon)
        {
            using NpgsqlConnection connection = new(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affected = await connection.ExecuteAsync(
                "INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)",
                new { coupon.ProductName, coupon.Description, coupon.Amount });

            return affected != 0;
        }

        public async Task<bool> DeleteDiscountAsync(string productName)
        {
            using NpgsqlConnection connection = new(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affected = await connection.ExecuteAsync(
                "DELETE FROM Coupon WHERE ProductName = @ProductName",
                new { ProductName = productName });

            return affected != 0;
        }

        public async Task<Coupon> GetDiscountAsync(string productName)
        {
            using NpgsqlConnection connection = new(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            Coupon coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(
                "SELECT * FROM Coupon WHERE ProductName = @ProductName",
                new { ProductName = productName });

            return coupon ?? new Coupon
            {
                ProductName = "No discount",
                Description = "No discount description",
                Amount = 0,
            };
        }

        public async Task<bool> UpdateDiscountAsync(Coupon coupon)
        {
            using NpgsqlConnection connection = new(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            int affected = await connection.ExecuteAsync(
                "UPDATE Coupon SET ProductName=@ProductName, Description = @Description, Amount = @Amount WHERE Id = @Id",
                new { coupon.ProductName, coupon.Description, coupon.Amount, coupon.Id });

            return affected != 0;
        }
    }
}
