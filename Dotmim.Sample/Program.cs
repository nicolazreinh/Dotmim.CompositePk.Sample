namespace Dotmim.Sample
{
    public class Program
    {
        private static readonly string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Trusted_Connection=Yes;Connect Timeout=120;TrustServerCertificate=Yes;";
        private static readonly string _syncServerUrl = "https://localhost:5001/api/sync";
        private static readonly string _firstClientDatabaseName = "Client1DB";
        private static readonly string _secondClientDatabaseName = "Client2DB";

        private static async Task Main(string[] args)
        {
            try
            {
                var firstClientConnectionString = _connectionString.Replace("master", _firstClientDatabaseName);
                var secondClientConnectionString = _connectionString.Replace("master", _secondClientDatabaseName);

                // Create two database with purcahse
                HelperMethods.ExcuteCreateDatabase(_connectionString, _firstClientDatabaseName);
                HelperMethods.ExcuteCreateDatabase(_connectionString, _secondClientDatabaseName);

                // Sync Database for the first time
                await HelperMethods.SyncDatabaseAsync(firstClientConnectionString, _syncServerUrl, Console.WriteLine);
                await HelperMethods.SyncDatabaseAsync(secondClientConnectionString, _syncServerUrl, Console.WriteLine);

                // Add purchase for first client
                HelperMethods.AddPurchase(
                    firstClientConnectionString,
                    1,
                    1,
                    1,
                    1,
                    200.50m,
                    "test from client1",
                    true);

                // Add purchase for second client
                HelperMethods.AddPurchase(
                    secondClientConnectionString,
                    1,
                    1,
                    1,
                    1,
                    700.50m,
                    "test from client2",
                    true);
            }
            finally
            {
                // Delete database for clean up
                HelperMethods.DeleteDatabase(_connectionString, _firstClientDatabaseName);
                HelperMethods.DeleteDatabase(_connectionString, _secondClientDatabaseName);
            }
        }
    }
}