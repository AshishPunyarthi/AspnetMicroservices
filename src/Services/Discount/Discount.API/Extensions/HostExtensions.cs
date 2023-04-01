using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost CreateDatabaseSchema<TContext>(this IHost host, int retryAttempts = 3)
        {
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();
                ILogger logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation($"Database schema creation started...attempts remaining {retryAttempts}");

                    using NpgsqlConnection connection = new(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

                    connection.Open();

                    using NpgsqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT EXISTS ( " +
                                            "SELECT FROM pg_tables " +
                                            "WHERE schemaname = 'public' AND tablename = 'coupon'" +
                                            " );";

                    bool tableExists = Convert.ToBoolean(command.ExecuteScalar());

                    if (tableExists)
                    {
                        logger.LogInformation("Database schema creation skipped as already exists");
                    }
                    else
                    {
                        command.CommandText = "CREATE TABLE coupon (" +
                                                "ID SERIAL PRIMARY KEY  NOT NULL," +
                                                "ProductName VARCHAR(24)  NOT NULL," +
                                                "Description TEXT," +
                                                "Amount INT" +
                                                ");";
                        _ = command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) " +
                                                "VALUES ('IPhone X', 'IPhone Discount', 150);";
                        _ = command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) " +
                                                "VALUES ('Samsung 10', 'Samsung Discount', 100);";
                        _ = command.ExecuteNonQuery();

                        logger.LogInformation("Database schema creation done");
                    }
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occurred while creating database schema");

                    if (retryAttempts > 0) // as db container might not be running till now
                    {
                        retryAttempts--;
                        Thread.Sleep(2000);
                        _ = CreateDatabaseSchema<TContext>(host, retryAttempts);
                    }
                }
            }

            return host;
        }
    }
}
