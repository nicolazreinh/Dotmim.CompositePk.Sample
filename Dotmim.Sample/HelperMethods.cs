using Dotmim.Sample.Properties;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Dotmim.Sample
{
    public class HelperMethods
    {
        public static void CreateDatabases(string _connectionString, string _firstClientDatabaseName, string _secondClientDatabaseName, string _syncClientDatabaseName)
        {
            ExcuteCreateDatabase(_connectionString, _firstClientDatabaseName);
            ExcuteCreateDatabase(_connectionString, _secondClientDatabaseName);
            CreateSyncDatabase(_connectionString, _syncClientDatabaseName);
        }

        public static void CleanUp(string _connectionString, string _firstClientDatabaseName, string _secondClientDatabaseName, string _syncClientDatabaseName)
        {
            DeleteDatabase(_connectionString, _firstClientDatabaseName);
            DeleteDatabase(_connectionString, _secondClientDatabaseName);
            DeleteDatabase(_connectionString, _syncClientDatabaseName);
        }

        public static void AddPurchaseForBothClientsWIthTheSamePurchasPrimaryKey(string firstClientConnectionString, string secondClientConnectionString)
        {
            // Add purchase for first client
            AddPurchase(firstClientConnectionString,
                        1,
                        1,
                        1,
                        1,
                        200.50m,
                        "test from client1",
                        true);

            // Add purchase for second client
            AddPurchase(secondClientConnectionString,
                        1,
                        1,
                        1,
                        1,
                        700.50m,
                        "test from client2",
                        true);
        }

        public static async Task SyncDatabaseAsync(string clientConnectionString, string serverAddress, string scopeName, Action<object> action)
        {
            using var httpClient = new HttpClient();
            await SyncHelper.SyncDatabaseFirstTimeAsync(httpClient, scopeName, clientConnectionString, serverAddress, action);

            await SyncHelper.SyncDatabase(httpClient, scopeName, clientConnectionString, serverAddress, action);
        }

        public static async Task SyncBothClientsAgain(
            string firstClientConnectionString,
            string secondClientConnectionString,
            string syncServerUrl,
            string scopeName)
        {
            var httpClient = new HttpClient();
            await SyncHelper.SyncDatabase(
                httpClient,
                scopeName,
                firstClientConnectionString,
                syncServerUrl,
                (result) => Debug.WriteLine(result));

            await SyncHelper.SyncDatabase(
                httpClient,
                scopeName,
                secondClientConnectionString,
                syncServerUrl,
                (result) => Debug.WriteLine(result));
        }

        private static void ExcuteCreateDatabase(string databaseConnectionString, string databaseName)
        {
            using var connection = new SqlConnection(databaseConnectionString);
            connection.Open();

            var databaseQuery = Resources.schema.Replace("@DatabaseName", databaseName);

            try
            {
                var parts = databaseQuery.Split(new[] { "**GO**" }, StringSplitOptions.None);

                foreach (var part in parts)
                {
                    using var command = new SqlCommand(part, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("There is already an object named"))
                    throw;
            }
        }

        private static void CreateSyncDatabase(string connectionString, string _syncClientDatabaseName)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var databaseQuery = Resources.schemaforsyncdb.Replace("@DatabaseName", _syncClientDatabaseName);

            try
            {
                var parts = databaseQuery.Split(new[] { "**GO**" }, StringSplitOptions.None);

                foreach (var part in parts)
                {
                    using var command = new SqlCommand(part, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("There is already an object named"))
                    throw;
            }
        }

        private static void AddPurchase(
            string connectionString,
            int fkVendorId,
            int fkPaymentTypeId,
            int fkItemId,
            int fkCurrencyId,
            decimal purchaseAmount,
            string description,
            bool mark)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var insertQuery = "INSERT INTO [dbo].[tbl_purchases] " +
                                "([fk_vendor_id], [fk_payment_type_id], [fk_item_id], [purchase_datetime], [fk_currency_id], [purchase_amount], [description], [fk_user_id], [mark]) " +
                                "VALUES " +
                                "(@fkVendorId, @fkPaymentTypeId, @fkItemId, @purchaseDateTime, @fkCurrencyId, @purchaseAmount, @description, @fkUserId, @mark)";

                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@fkVendorId", fkVendorId);
                command.Parameters.AddWithValue("@fkPaymentTypeId", fkPaymentTypeId);
                command.Parameters.AddWithValue("@fkItemId", fkItemId);
                command.Parameters.AddWithValue("@purchaseDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@fkCurrencyId", fkCurrencyId);
                command.Parameters.AddWithValue("@purchaseAmount", purchaseAmount);
                command.Parameters.AddWithValue("@description", description);
                command.Parameters.AddWithValue("@fkUserId", 1);
                command.Parameters.AddWithValue("@scannedItem", DBNull.Value);
                command.Parameters.AddWithValue("@mark", mark);

                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("New purchase inserted successfully!");
                }
                else
                {
                    const string msg = "No rows inserted. Something went wrong.";
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                throw;
            }
        }

        private static void DeleteDatabase(string connectionString, string databaseName)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string disconnectQuery = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            using (var disconnectCommand = new SqlCommand(disconnectQuery, connection))
            {
                disconnectCommand.ExecuteNonQuery();
            }

            string deleteQuery = $"DROP DATABASE [{databaseName}]";
            using var deleteCommand = new SqlCommand(deleteQuery, connection);
            deleteCommand.ExecuteNonQuery();
        }
    }
}
