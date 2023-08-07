using System.Diagnostics;

namespace Dotmim.Sample
{
    public class Program
    {
        private static readonly string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Trusted_Connection=Yes;Connect Timeout=120;TrustServerCertificate=Yes;";
        private static readonly string _syncServerUrl = "https://localhost:5001/api/sync";
        private static readonly string _firstClientDatabaseName = "Client1DB";
        private static readonly string _secondClientDatabaseName = "Client2DB";
        private static readonly string _syncClientDatabaseName = "TestSyncDB";
        private static readonly string _scopeName = "defaultScope";

        private static async Task Main(string[] args)
        {
            try
            {
                var firstClientConnectionString = _connectionString.Replace("master", _firstClientDatabaseName);
                var secondClientConnectionString = _connectionString.Replace("master", _secondClientDatabaseName);

                // Create two database with purcahse
                HelperMethods.CreateDatabases(_connectionString, _firstClientDatabaseName, _secondClientDatabaseName, _syncClientDatabaseName);

                // Sync Database for the first time
                await HelperMethods.SyncDatabaseAsync(firstClientConnectionString, _syncServerUrl, _scopeName, (result) => Debug.WriteLine(result));
                await HelperMethods.SyncDatabaseAsync(secondClientConnectionString, _syncServerUrl, _scopeName, (result) => Debug.WriteLine(result));

                // Add purchase for both client with the same purchase primary key
                HelperMethods.AddPurchaseForBothClientsWIthTheSamePurchasPrimaryKey(firstClientConnectionString, secondClientConnectionString);

                // Make sync for both clients again
                await HelperMethods.SyncBothClientsAgain(firstClientConnectionString, secondClientConnectionString, _syncServerUrl, _scopeName);
            }
            finally
            {
                // Delete database for clean up
                HelperMethods.CleanUp(_connectionString, _firstClientDatabaseName, _secondClientDatabaseName, _syncClientDatabaseName);
            }
        }
    }
}