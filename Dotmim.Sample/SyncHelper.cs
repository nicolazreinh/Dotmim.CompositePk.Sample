using Dotmim.Sample.Properties;
using Dotmim.Sync;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Web.Client;
using Microsoft.Data.SqlClient;

namespace Dotmim.Sample
{
    public class SyncHelper
    {
        public static async Task SyncDatabaseFirstTimeAsync(
            HttpClient client,
            string scopeName,
            string clientConnectionString,
            string syncServerAddress,
            Action<object> postSyncOperation)
        {
            var agent = CreateAgent(client, clientConnectionString, syncServerAddress);

            var s1 = await agent.SynchronizeAsync(scopeName);

            postSyncOperation(s1);

            FakeUpdate(clientConnectionString);

            //TODO: We need to change the FakeUpdate to this
            //await agent.LocalOrchestrator.UpdateUntrackedRowsAsync();

            var s2 = await agent.SynchronizeAsync(scopeName);

            postSyncOperation(s2);
        }

        public static async Task SyncDatabase(
            HttpClient httpClient,
            string scopeName,
            string clientConnectionString,
            string syncServerAddress,
            Action<object> postSyncOperation,
            SyncParameters parameters = null)
        {
            var agent = CreateAgent(httpClient, clientConnectionString, syncServerAddress);

            var sync = parameters == null ? await agent.SynchronizeAsync(scopeName, parameters) : await agent.SynchronizeAsync(scopeName);

            postSyncOperation(sync);
        }

        private static SyncAgent CreateAgent(HttpClient client, string clientConnectionString, string syncServerAddress)
        {
            var serverOrchestrator = new WebRemoteOrchestrator(syncServerAddress)
            {
                HttpClient = client
            };

            var clientProvider = new SqlSyncChangeTrackingProvider(clientConnectionString + "TrustServerCertificate=Yes;");

            var localOrchestrator = new LocalOrchestrator(clientProvider, serverOrchestrator.Options);

            return new SyncAgent(localOrchestrator, serverOrchestrator);
        }

        private static void FakeUpdate(string clientConnectionString)
        {
            using var connection = new SqlConnection(clientConnectionString);
            connection.Open();

            var query = Resources.fakeupdate;

            using var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
        }
    }
}