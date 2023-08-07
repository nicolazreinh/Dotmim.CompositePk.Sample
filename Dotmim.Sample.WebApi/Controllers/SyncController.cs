using Dotmim.Sync;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Web.Server;
using Microsoft.AspNetCore.Mvc;

namespace Dotmim.Sample.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SyncController : Controller
    {
        private readonly SqlSyncChangeTrackingProvider sqlSyncChangeTrackingProvider;
        private readonly IConfiguration config;

        public SyncController(
            SqlSyncChangeTrackingProvider sqlSyncChangeTrackingProvider,
            IConfiguration config)
        {
            this.sqlSyncChangeTrackingProvider = sqlSyncChangeTrackingProvider;
            this.config = config;
        }

        [HttpPost]
        public async Task Post()
        {
            var scopeName = HttpContext.GetScopeName();

            if (string.IsNullOrWhiteSpace(scopeName))
                throw new ArgumentException(nameof(scopeName));

            var connectionString = config.GetConnectionString("SyncDatabaseConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(nameof(connectionString));

            // TODO: we need to enable snapshot. 
            var options = new SyncOptions { };
            var setup = new SyncSetup(new string[] { "tbl_purchases" });

            var webserverAgent = new WebServerAgent(
                sqlSyncChangeTrackingProvider,
                setup,
                options,
                new WebServerOptions(),
                scopeName);

            await webserverAgent.HandleRequestAsync(HttpContext)
                .ConfigureAwait(false);
        }
    }
}
