using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;

namespace btsmon.Command.Group
{
    /// <summary>
    ///     Base initialisation logic a BizTalk Group related command.
    /// </summary>
    public class BaseGroupCheck
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BaseGroupCheck(Environment environment)
        {
            Environment = environment;

            var connectionString =
                $"Integrated Security=SSPI;database={environment.MgmtDatabase};server={environment.GroupServer}" +
                (!string.IsNullOrEmpty(environment.GroupInstance) ? $"instance={environment.GroupInstance}" : "");

            Logger.Debug($"BaseGroupCheck constructor - connection string = '{connectionString}'");

            BtsCatExplorer = new BtsCatalogExplorer
            {
                ConnectionString = connectionString
            };
        }

        protected BtsCatalogExplorer BtsCatExplorer { get; }

        protected Environment Environment { get; }
    }
}