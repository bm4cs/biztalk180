using System.Collections.Generic;
using btsmon.Model;
using NLog;

namespace btsmon.Command.Machine
{
    public class WebServerProcessesCheck : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Environment _environment;

        public WebServerProcessesCheck(Environment environment)
        {
            _environment = environment;
        }

        public List<Remediation> Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
